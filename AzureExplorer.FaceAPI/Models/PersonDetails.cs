using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureExplorer.FaceAPI.Models
{
    public class PersonDetails
    {
        [JsonProperty("image")]
        public byte[] Image { get; set; }
        [JsonProperty("personId")]
        public string PersonId { get; set; }
        [JsonProperty("fName")]
        public string FName { get; set;}
        [JsonProperty("lName")]
        public string LName { get; set; }
        [JsonProperty("adhaarNo")]
        public string AdhaarNo { get; set; }

    }
}