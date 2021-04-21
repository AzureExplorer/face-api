using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureExplorer.FaceAPI.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AzureExplorer.FaceAPI.Controllers
{
    [RoutePrefix("api/AzureFace")]
    public class AzureFaceController : ApiController
    {
        private const string _persongroupName = "AZURE_EXPLORER_GROUP";
        private const string _persongroupPersonGroupId = "d5d705cc-5e15-41a6-8aa3-8a7818006247";

        private readonly string _SubscriptionKey;
        private readonly string _Endpoint;
        private readonly IFaceClient _faceClient;

        /// <summary>
        /// Constructor
        /// </summary>
        public AzureFaceController()
        {
            _SubscriptionKey = "69cc6f5e6f584d7499d638cdea7f6d66";
            _Endpoint = "https://facescannerdemo.cognitiveservices.azure.com/";
            _faceClient = Authenticate(_Endpoint, _SubscriptionKey);
        }
        /// <summary>
        /// Method to upload image and detect faces in image
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UploadAndDetectFaces")]
        public async Task<IList<DetectedFace>> UploadAndDetectFaces(PersonDetails personDetails)
        {
            //using (Stream imageFileStream = new MemoryStream(await GetImageAsByteArray()))
            using (Stream imageFileStream = new MemoryStream(personDetails.Image))
            {
                // The second argument specifies to return the faceId, while
                // the third argument specifies not to return face landmarks.
                IList<DetectedFace> faceList =
                    await _faceClient.Face.DetectWithStreamAsync(
                        imageFileStream, true, true);

                return faceList;
            }
        }
        /// <summary>
        /// Method to create Person group
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("CreatePersonandGroup")]
        public async Task<bool> CreatePersonandGroup()
        {
            bool result = false;
            IList<PersonGroup> persongroupList = await _faceClient.PersonGroup.ListAsync();
            if (persongroupList.Count == 0)
                await _faceClient.PersonGroup.CreateAsync(_persongroupPersonGroupId, _persongroupName);

            result = true;
            return result;
        }
        /// <summary>
        /// Method to add new person
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddPerson")]
        public async Task<string> AddPerson(PersonDetails personDetails)
        {
            // The second argument specifies to return the faceId, while
            // the third argument specifies not to return face landmarks.

            Person p = await _faceClient.PersonGroupPerson.CreateAsync(_persongroupPersonGroupId, Guid.NewGuid().ToString());
            //using (Stream imageFileStream = new MemoryStream(await GetImageAsByteArray()))
            using (Stream imageFileStream = new MemoryStream(personDetails.Image))
            {
                await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                    _persongroupPersonGroupId, p.PersonId, imageFileStream);
            }
            TrainPersonGroup();
            return p.PersonId.ToString();
        }
        /// <summary>
        /// Method to get all person details 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllPerson")]
        public async Task<IList<Person>> GetAllPerson()
        {
            IList<Person> people = null;
            // Get Groups and Person
            IList<PersonGroup> persongroupList = await _faceClient.PersonGroup.ListAsync();
            foreach (PersonGroup pg in persongroupList)
            {
                people = await _faceClient.PersonGroupPerson.ListAsync(pg.PersonGroupId);
                foreach (Person person in people)
                {
                    // Do something with each person
                }
            }
            return people;
        }
        /// <summary>
        /// Method to train the person group with any newly added person details.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("TrainPersonGroup")]
        public async Task TrainPersonGroup()
        {
            await _faceClient.PersonGroup.TrainAsync(_persongroupPersonGroupId);
            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await _faceClient.PersonGroup.GetTrainingStatusAsync(_persongroupPersonGroupId);
                Console.WriteLine($"Training status: {trainingStatus.Status}.");
                if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
            }
        }
        /// <summary>
        /// Method to identify a person
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("IdentifyPerson")]
        public async Task<string> IdentifyPerson(PersonDetails personDetails)
        {
            Person people = null;
            var detectedFaces = await UploadAndDetectFaces(personDetails);
            List<Guid> sourceFaces = new List<Guid>();
            if (detectedFaces.Any(f => f.FaceId.HasValue))
                sourceFaces.Add(detectedFaces[0].FaceId.Value);

            var similarResults = await _faceClient.Face.IdentifyAsync(sourceFaces, _persongroupPersonGroupId);

            if (similarResults.Count > 0 && similarResults[0].Candidates.Count > 0)
                people = new Person { PersonId = similarResults[0].Candidates[0].PersonId };
            if (people != null)
                return people.PersonId.ToString();
            return "";
        }
        /// <summary>
        /// Method to get an image byte array
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> GetImageAsByteArray()
        {
            using (WebClient client = new WebClient())
            {
                byte[] bytes = await client.DownloadDataTaskAsync("https://faceapiimagestorage2021.blob.core.windows.net/faceapiimagecontainer/detection1.jpg");
                return bytes;
            }
        }
        /// <summary>
        /// AUTHENTICATE: ses subscription key and region to create a client.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="key"></param>
        /// <returns></returns>        
        private IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
    }
}

