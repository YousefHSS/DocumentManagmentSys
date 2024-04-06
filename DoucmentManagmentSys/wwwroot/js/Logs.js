
function showLogs(docname) {

    //make ajax request to get the logs of the document
$.ajax({
        type: "GET",
        url: "HistoryLog/index",
    data: { doc_name: docname },
        success: function (data) {
            //display the logs in the logs page
           
        }
    });
}

