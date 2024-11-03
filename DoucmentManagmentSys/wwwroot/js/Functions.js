Forms = document.querySelectorAll('.password-protected-form, #PasswordProtectedForm');


Forms.forEach((form) => {
    form.addEventListener('submit', (event) => {
        event.preventDefault();

        const password = form.querySelector('#passwordPopup');
        const passwordVal = password.querySelector('input').value;


        if (passwordVal === '' || passwordVal === null || passwordVal === undefined) {
            password.style.display = 'block';
            //scroll to the password input
            password.scrollIntoView();

            return;
        }
        else {
            sessionStorage.setItem('password', passwordVal);
        }

        
        form.submit();
    });
});