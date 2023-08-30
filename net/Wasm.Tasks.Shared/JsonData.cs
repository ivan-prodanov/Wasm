using System.Runtime.Serialization;

namespace Wasm.Tasks.Shared
{
    [DataContract]
    public class ContentFile
    {
        public ContentFile(string name, long size, string hash)
        {
            this.Name = name;
            this.Size = size;
            this.Hash = hash;
        }

        [DataMember(Order = 0, Name = "name")]
        public string Name { get; set; }

        [DataMember(Order = 1, Name = "size")]
        public long Size { get; set; }

        [DataMember(Order = 2, Name = "hash")]
        public string Hash { get; set; }
    }
}
