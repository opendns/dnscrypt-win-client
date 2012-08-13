using System;

public class DNSPacket
{
    #region Members

    protected byte[] m_bPacket = null;
    protected String m_sName = "";

    protected byte[] m_bReceivedBytes = null;

    #endregion

    #region Classes

    public class PacketExplode
    {
        #region Members

        // Header parts
        byte[] m_bID = new byte[] { 0x00, 0x00 };
        bool m_bQR = false;
        int m_nOpcode = -1;
        bool m_bAA = false;
        bool m_bTC = false;
        bool m_bRD = false;
        bool m_bRA = false;
        int m_nZ = -1;
        int m_nRCode = -1;
        int m_nQDCount = -1;
        int m_nANCount = -1;
        int m_nNSCount = -1;
        int m_nARCount = -1;

        // Query stuff
        string m_sQName = string.Empty;
        int m_nQType = 0;
        int m_nQClass = 0;

        // RR stuff
        string m_sRRName = string.Empty;
        int m_nRRType = 0;
        int m_nRRClass = 0;
        int m_nTTL = 0;
        int m_nRDLength = 0;
        byte[] m_bRData = null;

        #endregion

        // Constructor - does full breakdown on construction
        public PacketExplode(byte[] bPacket)
        {
            if (bPacket == null)
                return;

            // If any part fails, -1 is returned, this allows
            // us to go as far as we can till we hit anything
            // malformed
            int nIndex = ExplodeHeader(bPacket);
            if (nIndex > -1)
            {
                // TODO: Only picks up last query
                for (int i = 0; i < m_nQDCount; i++)
                {
                    nIndex = ExplodeQueries(bPacket, nIndex);
                }

                // TODO: only does last RR
                if (nIndex > -1)
                {
                    for (int i = 0; i < m_nANCount; i++)
                    {
                        nIndex = ExplodeRR(bPacket, nIndex);
                    }
                }
            }
        }

        // Returns index to header end
        private int ExplodeHeader(byte[] bPacket)
        {
            // 6 * 16 bits = 12 bytes
            if (bPacket.Length > 12)
            {
                try
                {
                    // First 2 bytes
                    m_bID[0] = bPacket[0];
                    m_bID[1] = bPacket[1];

                    // Third byte
                    byte bCur = bPacket[2];
                    m_bQR = ((0x80 & bCur) > 0 ? true : false); // First bit
                    m_nOpcode = (int)(0x78 & bCur); // Next 4 bits
                    m_bAA = ((0x04 & bCur) > 0 ? true : false); // Next bit
                    m_bTC = ((0x02 & bCur) > 0 ? true : false); // Next bit
                    m_bRD = ((0x01 & bCur) > 0 ? true : false); // Next bit

                    // Fourth byte
                    bCur = bPacket[3];
                    m_bRA = ((0x80 & bCur) > 0 ? true : false); // First bit
                    m_nZ = (int)(0x70 & bCur); // Next 3 bits
                    m_nRCode = (int)(0x0F & bCur); // Next 4 bits

                    // Fifth & Sixth
                    m_nQDCount = bPacket[4];
                    m_nQDCount <<= 8;
                    m_nQDCount |= bPacket[5];

                    // Seventh & Eighth
                    m_nANCount = bPacket[6];
                    m_nANCount <<= 8;
                    m_nANCount |= bPacket[7];

                    // Ninth & Tenth
                    m_nNSCount = bPacket[8];
                    m_nNSCount <<= 8;
                    m_nNSCount |= bPacket[9];

                    // Eleventh & Twelvth
                    m_nARCount = bPacket[10];
                    m_nARCount <<= 8;
                    m_nARCount |= bPacket[11];
                }
                catch (Exception Ex)
                {
                    return -1;
                }

                return 12;
            }

            return -1;
        }

        // Returns index to query end
        private int ExplodeQueries(byte[] bPacket, int nOffset)
        {
            if (bPacket == null)
                return -1;

            if (nOffset < 0)
                return -1;

            // Get the QNAME
            try
            {
                int nMax = bPacket.Length;
                if (nMax <= nOffset)
                    return -1;

                // Get first len
                int nCurLen = (int)bPacket[nOffset++];
                int nCurIndex = 0;

                // Go till we hit the root null byte
                do
                {
                    if (nCurIndex == nCurLen)
                    {
                        nCurLen = (int)bPacket[nOffset++];
                        m_sQName += ".";
                        nCurIndex = -1;
                    }
                    else
                    {
                        // Taking a liberty with the conversion to char, but
                        // we don't really care about the host anyway
                        m_sQName += (char)bPacket[nOffset++];
                    }

                    nCurIndex++;
                }
                while (bPacket[nOffset] != 0x00);

                // Move past the null root byte
                nOffset++;

                // Get the QTYPE
                m_nQType = bPacket[nOffset++];
                m_nQType <<= 8;
                m_nQType |= bPacket[nOffset++];

                // Get the QCLASS
                m_nQClass = bPacket[nOffset++];
                m_nQClass <<= 8;
                m_nQClass |= bPacket[nOffset++];
            }
            catch (Exception Ex)
            {
                return -1;
            }

            return nOffset;
        }

        // Returns index to RR end
        private int ExplodeRR(byte[] bPacket, int nOffset)
        {
            if (bPacket == null)
                return -1;

            if (nOffset < 0)
                return -1;

            // Get the NAME
            try
            {
                // Check for name compression
                byte bCur = bPacket[nOffset];
                if ((bCur & 0xC0) > 0)
                {
                    // This is compressed, just use offset
                    m_sRRName = m_sQName;

                    // Get past
                    nOffset += 2;
                }
                else
                {
                    int nMax = bPacket.Length;
                    if (nMax <= nOffset)
                        return -1;

                    // Get first len
                    int nCurLen = (int)bPacket[nOffset++];
                    int nCurIndex = 0;

                    // Go till we hit the root null byte
                    do
                    {
                        if (nCurIndex == nCurLen)
                        {
                            nCurLen = (int)bPacket[nOffset++];
                            m_sRRName += ".";
                            nCurIndex = -1;
                        }
                        else
                        {
                            // Taking a liberty with the conversion to char, but
                            // we don't really care about the host anyway
                            m_sRRName += (char)bPacket[nOffset++];
                        }

                        nCurIndex++;
                    }
                    while (bPacket[nOffset] != 0x00);

                    // Move past the null root byte
                    nOffset++;
                }

                // Get the TYPE
                m_nRRType = bPacket[nOffset++];
                m_nRRType <<= 8;
                m_nRRType |= bPacket[nOffset++];

                // Get the CLASS
                m_nRRClass = bPacket[nOffset++];
                m_nRRClass <<= 8;
                m_nRRClass |= bPacket[nOffset++];

                // Get the TTL
                m_nTTL = bPacket[nOffset++];
                m_nTTL <<= 8;
                m_nTTL |= bPacket[nOffset++];
                m_nTTL <<= 16;
                m_nTTL |= bPacket[nOffset++];
                m_nTTL <<= 24;
                m_nTTL |= bPacket[nOffset++];

                // Get the RDLENGTH
                m_nRDLength = bPacket[nOffset++];
                m_nRDLength <<= 8;
                m_nRDLength |= bPacket[nOffset++];

                // Get the RDATA
                m_bRData = new byte[m_nRDLength];
                for (int i = 0; i < m_nRDLength; i++)
                {
                    m_bRData[i] = bPacket[nOffset++];
                }

            }
            catch (Exception Ex)
            {
                return -1;
            }

            return nOffset;
        }

        #region Access Functions

        public bool IsResponse()
        {
            return m_bQR;
        }

        public byte[] GetIP()
        {
            return m_bRData;
        }

        public string GetHostString()
        {
            return this.m_sQName;
        }

        public byte[] GetRRBytes()
        {
            return this.m_bRData;
        }

        public bool IsTruncated()
        {
            return this.m_bTC;
        }

        #endregion

    }

    #endregion

    #region Methods

    public DNSPacket()
    {
        m_sName = "";
        m_bReceivedBytes = new byte[0];
    }

    // Forms normal DNS packet and sets member
    public void CreateDNSPacket(string sURL, byte bType)
    {
        this.m_sName = sURL;

        // Calculate variable fields first

        // Break it up by periods
        string[] sSplit = sURL.Split('.');

        if (sSplit.Length < 2)
            return;

        // Name
        int nNameByteCount = 0;
        for (int i = 0; i < sSplit.Length; i++)
        {
            nNameByteCount += System.Text.ASCIIEncoding.Default.GetByteCount(sSplit[i]);
        }

        // Calculate whole packet size

        // Name (plus a byte for each segment for length)
        int nPacketLen = nNameByteCount;
        nPacketLen += sSplit.Length;

        // 17 for everyone else TEMP!!!
        nPacketLen += 17;

        // Allocate and build packet
        int nCurPacketIndex = 0;
        m_bPacket = new byte[nPacketLen];

        // Identifier
        m_bPacket[nCurPacketIndex++] = 0xba;
        m_bPacket[nCurPacketIndex++] = 0xad;

        // Header Block:
        // QueryType 1bit (0 query, 1 response)
        // Opcode 4bits (0 query, 1 inverse)
        // AA 1bit (response only)
        // TC 1bit (Truncated)
        // RD 1bit (Recursion)
        // RA 1bit (Recursion Available - response only)
        m_bPacket[nCurPacketIndex++] = 0x01;
        m_bPacket[nCurPacketIndex++] = 0x00;

        // Question count
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x01;

        // Reserved
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;

        // Don't set last bit to denote extended packet (kind of)
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;

        // Name
        // TODO: put in check for too long (255!)
        for (int i = 0; i < sSplit.Length; i++)
        {
            // Get the bytes (use ASCII per RFC 1035 for domain names)
            byte[] bCurBuf = System.Text.ASCIIEncoding.Default.GetBytes(sSplit[i]);

            // Put on the length for this segment
            byte bLen = Convert.ToByte(bCurBuf.Length);
            m_bPacket[nCurPacketIndex++] = bLen;

            // Put on the segment
            foreach (byte bCur in bCurBuf)
            {
                m_bPacket[nCurPacketIndex++] = bCur;
            }
        }

        // Last (root) len always 0
        m_bPacket[nCurPacketIndex++] = 0x00; // No length

        // Type
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = bType;

        // Class (internet for now)
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x01;
    }

    // Forms normal DNS packet and sets member
    public void CreateRandomDNSPacket(byte bType)
    {
        // Create random string 12 chars long
        string sURL = "www."; 
        sURL += CreateRandomString(12);
        sURL += ".com";
        this.m_sName = sURL;

        // Calculate variable fields first

        // Break it up by periods
        string[] sSplit = sURL.Split('.');

        if (sSplit.Length < 2)
            return;

        // Name
        int nNameByteCount = 0;
        for (int i = 0; i < sSplit.Length; i++)
        {
            nNameByteCount += System.Text.ASCIIEncoding.Default.GetByteCount(sSplit[i]);
        }

        // Calculate whole packet size

        // Name (plus a byte for each segment for length)
        int nPacketLen = nNameByteCount;
        nPacketLen += sSplit.Length;

        // 17 for everyone else TEMP!!!
        nPacketLen += 17;

        // Allocate and build packet
        int nCurPacketIndex = 0;
        m_bPacket = new byte[nPacketLen];

        // Identifier
        m_bPacket[nCurPacketIndex++] = 0xba;
        m_bPacket[nCurPacketIndex++] = 0xad;

        // Status block
        m_bPacket[nCurPacketIndex++] = 0x01;
        m_bPacket[nCurPacketIndex++] = 0x00;

        // Question count
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x01;

        // Reserved
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x00;

        // Denotes extended packet (kind of)
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x01;

        // Name
        // TODO: put in check for too long (255!)
        for (int i = 0; i < sSplit.Length; i++)
        {
            // Get the bytes (use ASCII per RFC 1035 for domain names)
            byte[] bCurBuf = System.Text.ASCIIEncoding.Default.GetBytes(sSplit[i]);

            // Put on the length for this segment
            byte bLen = Convert.ToByte(bCurBuf.Length);
            m_bPacket[nCurPacketIndex++] = bLen;

            // Put on the segment
            foreach (byte bCur in bCurBuf)
            {
                m_bPacket[nCurPacketIndex++] = bCur;
            }
        }

        // Last (root) len always 0
        m_bPacket[nCurPacketIndex++] = 0x00; // No length

        // Type
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = bType;

        // Class (internet for now)
        m_bPacket[nCurPacketIndex++] = 0x00;
        m_bPacket[nCurPacketIndex++] = 0x01;
    }

    // Returns 0 for null packet
    public int GetPacketLen()
    {
        if(this.m_bPacket!=null)
            return this.m_bPacket.Length;
        else
            return 0;
    }

    // Check for null packet
    public byte[] GetPacket()
    {
        return this.m_bPacket;
    }

    public string GetPacketURL()
    {
        return this.m_sName;
    }

    public void SetReceivePacket(byte[] bReceive)
    {
        m_bReceivedBytes = bReceive;
    }

    public byte[] GetResponsePacket()
    {
        return m_bReceivedBytes;
    }

    // Check for null returned
    public byte[] GetIPFromResponse()
    {
        PacketExplode pe = new PacketExplode(m_bReceivedBytes);
        if (pe.IsResponse())
        {
            return pe.GetIP();
        }
        else
        {
            return null;
        }
    }

    #region Utility

    protected string CreateRandomString(int nLen)
    {
        string sOut = "";

        // Generate a rand seeded by minute AND milli so it is at least kindof random
        Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);
        for (int i = 0; i < nLen; i++)
        {
            // Add 65 to get into the ASCII range
            int nCur = rand.Next(0, 25) + 65;
            sOut += Convert.ToChar(nCur);
        }

        sOut = sOut.ToLower();

        return sOut;
    }

    protected byte[] GetBytesFromInt16(Int16 nIn)
    {
        byte[] bOut = new byte[2];
        if (nIn < 255)
        {
            bOut[0] = 0x00;
            bOut[1] = (byte)nIn;
        }
        else
        {
            bOut[0] = (byte)(nIn >> 8);
            bOut[1] = (byte)(nIn & 0x00ff);
        }

        return bOut;
    }

    #endregion

    #endregion

}

public class EDNSPacket : DNSPacket
{
    #region Members

    byte[] m_bOPDATA = null;
    byte[] m_bMachineID = null;

    public EDNSPacket()
    {
        m_bReceivedBytes = new byte[0];
    }

    // Only allows (Currently) for one RDATA section
    public void CreateEDNSPacket(string sURL, byte bType, byte[] bOPDATA)
    {
        // Save for later
        m_bOPDATA = bOPDATA;

        GetDeviceIDFromOPDATA(m_bOPDATA);

        // Start with a normal dns packet
        CreateDNSPacket(sURL, bType);

        // Check if successful
        if(GetPacketLen()>0)
        {
            // Figure out new size
            int nPacketGrow = 0;

            // Seemingly 2 for end of old packet? + IANA OPCODE (0x29) + 6 Bytes for unknown
            nPacketGrow += 10;

            // Now calculate extended (data + 1 for len)
            Int16 OPDATA_len = Convert.ToInt16(bOPDATA.Length); // Can't be more than 512 anyway
            nPacketGrow += OPDATA_len + 1;

            // Allocate larger packet (TEMP)
            byte[] EDNSPacket = new byte[GetPacketLen() + nPacketGrow];

            // Copy base class packet first
            m_bPacket.CopyTo(EDNSPacket, 0);

            // Append new Extended data
            int nPacketOffset = m_bPacket.Length;

            // Empty Root Domain
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;

            // IANA OPCODE (0x29 = 41)
            EDNSPacket[nPacketOffset++] = 0x29;

            // Senders payload size (512)
            EDNSPacket[nPacketOffset++] = 0x02;
            EDNSPacket[nPacketOffset++] = 0x00;

            // Extended RCODE and flags
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;

            // Data len
            byte[] bLen = GetBytesFromInt16(OPDATA_len);
            for(int i=0; i<bLen.Length; i++)
            {
                EDNSPacket[nPacketOffset++] = bLen[i];
            }

            // Data
            for (int i = 0; i < OPDATA_len; i++)
            {
                EDNSPacket[nPacketOffset++] = bOPDATA[i];
            }

            // IMPORTANT!!! must set this bit to be EDNS
            EDNSPacket[11] |= 0x01;

            // Now set as member packet
            this.m_bPacket = EDNSPacket;
        }
    }

    // Override to allow input of regular packet and append ENDS0 data to it
    public void CreateEDNSPacket(byte[] bInputPacket, int nLen, byte[] bOPDATA)
    {
        // Save for later
        m_bOPDATA = bOPDATA;

        // Use only the length of the input buffer as told to us, as it
        // may physically be bigger and only partially used
        if (nLen > 0)
        {
            // Figure out new size
            int nPacketGrow = 0;

            // Seemingly 2 for end of old packet? + IANA OPCODE (0x29) + 6 Bytes for unknown
            nPacketGrow += 10;

            // Now calculate extended (data + 1 for len)
            Int16 OPDATA_len = Convert.ToInt16(bOPDATA.Length); // Can't be more than 512 anyway
            nPacketGrow += OPDATA_len + 1;

            // Allocate larger packet (TEMP)
            byte[] EDNSPacket = new byte[nLen + nPacketGrow];

            // Copy input packet first
            for (int i = 0; i < nLen; i++)
            {
                EDNSPacket[i] = bInputPacket[i];
            }

            // IMPORTANT!!! must set this bit to be EDNS
            EDNSPacket[11] |= 0x01;

            // Append new Extended data
            int nPacketOffset = nLen;

            // Empty Root Domain
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;

            // IANA OPCODE (0x29 = 41)
            EDNSPacket[nPacketOffset++] = 0x29;

            // Senders payload size (512)
            EDNSPacket[nPacketOffset++] = 0x02;
            EDNSPacket[nPacketOffset++] = 0x00;

            // Extended RCODE and flags
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;
            EDNSPacket[nPacketOffset++] = 0x00;

            // Data len
            byte[] bLen = GetBytesFromInt16(OPDATA_len);
            for (int i = 0; i < bLen.Length; i++)
            {
                EDNSPacket[nPacketOffset++] = bLen[i];
            }

            // Data
            for (int i = 0; i < OPDATA_len; i++)
            {
                EDNSPacket[nPacketOffset++] = bOPDATA[i];
            }

            // Now set as member packet
            this.m_bPacket = EDNSPacket;
        }
        else
        {
            throw new Exception("Input length to CreateDNSPacket() was 0");
        }
    }

    // Same as regular, but creates a ransom string for the domain to resolve
    public void CreateRandomEDNSPacket(byte bType, byte[] bOPDATA)
    {
        // Create random string 12 chars long
        string sURL = "www."; 
        sURL += CreateRandomString(12);
        sURL += ".com";

        CreateEDNSPacket(sURL, bType, bOPDATA);
    }

    // Override to allow just passing in the machine_id but using standard OPDATA
    public void CreateEDNSPacketMachineID(string sURL, byte bType, byte[] bMachineID)
    {
        // Generic EDNS packet in the format of OpenDNS
        m_bOPDATA = new byte[] { 0x00, 0x04, 0x00, 0x0f, 0x4f, 0x70, 0x65, 0x6e, 0x44, 0x4e, 0x53, 0xaa, 0xaa, 0xbb, 0xbb, 0xcc, 0xcc, 0xdd, 0xdd };

        // Start with a normal dns packet
        CreateDNSPacket(sURL, bType);
        
        // Add on the machine_id
        if (bMachineID.Length == 8)
        {
            // Save for quick compare later
            m_bMachineID = bMachineID;

            // Load it at offset 11
            for (int i = 0; i < 8; i++)
                m_bOPDATA[i + 11] = bMachineID[i];

            // Check if successful
            if (GetPacketLen() > 0)
            {
                // Figure out new size
                int nPacketGrow = 0;

                // Seemingly 2 for end of old packet? + IANA OPCODE (0x29) + 6 Bytes for unknown
                nPacketGrow += 10;

                // Now calculate extended (data + 1 for len)
                Int16 OPDATA_len = Convert.ToInt16(m_bOPDATA.Length); // Can't be more than 512 anyway
                nPacketGrow += OPDATA_len + 1;

                // Allocate larger packet (TEMP)
                byte[] EDNSPacket = new byte[GetPacketLen() + nPacketGrow];

                // Copy base class packet first
                m_bPacket.CopyTo(EDNSPacket, 0);

                // Append new Extended data
                int nPacketOffset = m_bPacket.Length;

                // Empty Root Domain
                EDNSPacket[nPacketOffset++] = 0x00;
                EDNSPacket[nPacketOffset++] = 0x00;

                // IANA OPCODE (0x29 = 41)
                EDNSPacket[nPacketOffset++] = 0x29;

                // Senders payload size (512)
                EDNSPacket[nPacketOffset++] = 0x02;
                EDNSPacket[nPacketOffset++] = 0x00;

                // Extended RCODE and flags
                EDNSPacket[nPacketOffset++] = 0x00;
                EDNSPacket[nPacketOffset++] = 0x00;
                EDNSPacket[nPacketOffset++] = 0x00;
                EDNSPacket[nPacketOffset++] = 0x00;

                // Data len
                byte[] bLen = GetBytesFromInt16(OPDATA_len);
                for (int i = 0; i < bLen.Length; i++)
                {
                    EDNSPacket[nPacketOffset++] = bLen[i];
                }

                // Data
                for (int i = 0; i < OPDATA_len; i++)
                {
                    EDNSPacket[nPacketOffset++] = m_bOPDATA[i];
                }

                // IMPORTANT!!! must set this bit to be EDNS
                EDNSPacket[11] |= 0x01;

                // Now set as member packet
                this.m_bPacket = EDNSPacket;
            }
        }
    }

    #region Verification Functions

    private void GetDeviceIDFromOPDATA(byte[] bOPDATA)
    {
        m_bMachineID[0] = bOPDATA[11];
        m_bMachineID[1] = bOPDATA[12];
        m_bMachineID[2] = bOPDATA[13];
        m_bMachineID[3] = bOPDATA[14];
        m_bMachineID[4] = bOPDATA[15];
        m_bMachineID[5] = bOPDATA[16];
        m_bMachineID[6] = bOPDATA[17];
        m_bMachineID[7] = bOPDATA[18];
    }

    #endregion

    #endregion
}

