
function GetVersions( id,context){
    //make ajax request to get the Versions of the document

    $.ajax({
        type: "POST",
        url: "ArchivedDocuments/GetVersions",
        data: { doc_id: id },
        success: function (data) {
            //display the logs in the logs page wrapped in option tags
            var options = '';
            options='<option value="Latest">Latest</option>'
            data.forEach(function (data) {
                 options += '<option value="'+data+'">'+data+'</option>'
                
            });
            context.innerHTML = options;

        }
    });

}