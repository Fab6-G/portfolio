using System;
using UnityEngine;

namespace MoodleAPI.Operations.ToolStarkdevicesSetUserID
{
    public class ToolStarkdevicesSetUserIDRequest : PostRequest
    {
        public override Uri BuildUri(_StarkAPP.Scripts.MoodleAPI.MoodleAPI api)
        {
            return new UriQueryBuilder($"{api.moodleUrl}/webservice/rest/server.php")
                .AddQuery("wstoken", api.token)
                .AddQuery("wsfunction", "tool_starkdevices_set_user_id")
                .AddQuery("moodlewsrestformat", "json")
                .AddQuery("deviceid", api.usersDeviceID)
                .AddQuery("scenarioid", api.scenarioID)
                .AddQuery("courseid", api.activeCampaignID)
                .AddQuery("moduleid", api.moduleID)
                .Build();
        }

        public override WWWForm BuildFormData(_StarkAPP.Scripts.MoodleAPI.MoodleAPI api)
        {
            var wwwForm = new WWWForm();

            //wwwForm.AddField("users[0][id]", api.userID);
            //wwwForm.AddField("users[0][firstname]", firstname);

            return wwwForm;
        }
    }
}
