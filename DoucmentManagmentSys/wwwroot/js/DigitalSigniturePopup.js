//on form submission
document.addEventListener("DOMContentLoaded", function (event) {


    const forms = document.querySelectorAll('#Approve'); // Replace with your form's ID

    if (forms) {

        forms.forEach((form) => {

            form.addEventListener('submit', (event) => {
                
                event.preventDefault(); // This stops the form from submitting
                ConfirmApprove(form);
              
                //Send To ConfirmAction


            });

        });

    }




});

function ConfirmApprove(form) {

    //intercept form submit and send it to the controller
    const FileName = form["FileName"].value;
    const id = form["id"].value;
            $.ajax({
                url: "/Home/ConfirmApprove",
                type: "POST",
                beforeSend: function (request) {
                    request.setRequestHeader("RequestVerificationToken", $("[name='__RequestVerificationToken']").val());
                },
                data: { FileName: FileName, id: id },
                success: function (data) {

                    // replace the div with the new data
                    $("#PopupPlaceHolder").html(data);


                }
            });
}


function HidePopup(element) {
    element.parentElement.classList.add('d-none');
}