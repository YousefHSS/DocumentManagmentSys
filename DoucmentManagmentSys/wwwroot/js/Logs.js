

document.getElementById('submitPassword').addEventListener('click', function () {
    const password = document.getElementById('password').value;

    // Add the password to the form
    const passwordInput = document.createElement('input');
    passwordInput.type = 'hidden';
    passwordInput.name = 'password';
    passwordInput.value = password;
    document.querySelector('.password-protected-form').appendChild(passwordInput);

    // Submit the form programmatically
    document.querySelector('.password-protected-form').submit();
});

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

