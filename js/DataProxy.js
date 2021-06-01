


function OnLoadAllProjects() {

    $.ajax({
        type: 'POST',
        url: API_ROOT + "GetAllProjects.aspx",

        data: {
            key: API_KEY,
            filterName: "status",
            filterValue: "dev"
        },

        success: function (response) {
            DebugDebug("DataProxy.OnLoadAllProjects: ");
            var result = JSON.parse(response).success;

            if (result ==
                "true") {
                var projects = JSON.parse(response).message;
                OnLoadProjects_CallBack(projects);
            } else {
                // do error message
            }

        },
        error: function (response) {
            Debug("DataProxy.UploadProject: Error, " + response);
            return response;
        }
    });
}
