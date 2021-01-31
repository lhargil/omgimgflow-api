using API.Data;
using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
{
    public class DataSeeder
    {
        public async static Task Seed(OmgImageServerDbContext omgImageServerDb)
        {
            if (!omgImageServerDb.OmgImages.Any())
            {
                var omgImage1 = new OmgImage("light-em-up.jpg", "Lightning over KL Skyline");
                omgImage1.Description = "This is a great lightning photo over Kuala Lumpur";

                var omgImage2 = new OmgImage("bantayan-island.jpg", "Bantayan Island port");
                omgImage2.Description = "Bantayan island port during sunset";
                omgImage2.AddTag("colorful");

                var omgImage3 = new OmgImage("Instagram-20140928-2.jpg", "Jump shot");
                omgImage3.Description = "Jump shot of a group of people";
                omgImage3.AddTag("tag 1");
                omgImage3.AddTag("tag 2");

                var omgImage4 = new OmgImage("the-exchange-106-3.jpg", "The Exchange 106");
                omgImage4.Description = "The Exchange 106 during twilight";
                omgImage4.AddTag("colorful");
                omgImage4.AddTag("twilight");

                var images = new List<OmgImage>
                {
                    omgImage1,
                    omgImage2,
                    omgImage3,
                    omgImage4
                };

                await omgImageServerDb.AddRangeAsync(images);
                await omgImageServerDb.SaveChangesAsync();
                
                /*
                    {
                        "id": "08d8c4ca-3a8d-4e47-87ad-f2998137e1e5",
                        "filename": "light-em-up.jpg",
                        "title": "Lightning over KL Skyline",
                        "description": "This is a great lightning photo over Kuala Lumpur",
                        "tags": []
                    },
                    {
                        "id": "08d8c4cc-0493-41c9-8990-84a38e585cc2",
                        "filename": "bantayan-island.jpg",
                        "title": "Bantayan Island port",
                        "description": "Bantayan island port during sunset",
                        "tags": [
                            "colorful"
                        ]
                    },
                    {
                        "id": "08d8c4cc-579c-474c-8be1-69cf8f57f3a3",
                        "filename": "Instagram-20140928-2.jpg",
                        "title": "Jump shot",
                        "description": "Jump shot of a group of people",
                        "tags": [
                            "tag 1",
                            "tag 2"
                        ]
                    },
                    {
                        "id": "08d8c4cd-0305-4b2c-8c90-4bd76ee96846",
                        "filename": "the-exchange-106-3.jpg",
                        "title": "The Exchange 106",
                        "description": "The Exchange 106 during twilight",
                        "tags": [
                            "colorful",
                            "twilight"
                        ]
                    }
                 */
            }
        }
    }
}
