using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Monad.FLParser
{
    internal class ProjectWriter
    {
        private readonly FileStream s;
        
        public ProjectWriter(FileStream stream)
        {
            s = stream;
        }

        ~ProjectWriter()
        {
            s.Close();
        }
        
        public void Write(Enums.Event id, int offset, byte data)
        {
            if (id < Enums.Event.Word)
            {
                s.Write(new byte[] { (byte)id, data }, offset, 2);
            }
        }

        public void Write(Enums.Event id, int offset, ushort data)
        {
            if (id >= Enums.Event.Word && id < Enums.Event.Int)
            {
                var buffer = new List<byte>();
                buffer.Add((byte)id);
                buffer.AddRange(BitConverter.GetBytes(data));
                s.Write(buffer.ToArray(), offset, 3);
            }
        }

        public void Write(Enums.Event id, int offset, uint data)
        {
            if (id >= Enums.Event.Int && id < Enums.Event.Text)
            {
                var buffer = new List<byte>();
                buffer.Add((byte)id);
                buffer.AddRange(BitConverter.GetBytes(data));
                s.Write(buffer.ToArray(), offset, 5);
            }
        }

        public void Write(Enums.Event id, int offset, string data, bool unicode)
        {
            if (id >= Enums.Event.Text)
            {
                int data_len = data.Length;
                do
                {
                    int towrite = data_len & 0x7F;
                    data_len >>= 7;
                    if (data_len > 0) towrite |= 0x80;
                    s.Write(new byte[] { Convert.ToByte(towrite) }, offset, 1);
                    offset++;
                } while (data_len > 0);

                var byteList = new List<byte>(data_len);
                if (unicode)
                {
                    byteList.AddRange(Encoding.Unicode.GetBytes(data.Trim()));
                }
                else
                {
                    byteList.AddRange(Encoding.ASCII.GetBytes(data.Trim()));
                }
                var byteArr = byteList.ToArray();
                var bytesLen = byteArr.Length;
                s.Write(byteArr, offset, bytesLen);
            }
        }

        public void Write(Enums.Event id, int offset, byte[] data)
        {
            if (id >= Enums.Event.Data)
            {
                int data_len = data.Length;
                do
                {
                    int towrite = data_len & 0x7F;
                    data_len >>= 7;
                    if (data_len > 0) towrite |= 0x80;
                    s.Write(new byte[] { Convert.ToByte(towrite) }, offset, 1);
                    offset++;
                } while (data_len > 0);

                s.Write(data, offset, data.Length);
            }
        }
    }
}
