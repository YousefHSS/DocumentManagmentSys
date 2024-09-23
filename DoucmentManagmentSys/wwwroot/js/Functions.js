Forms = document.querySelectorAll('.password-protected-form');


Forms.forEach((form) => {
    form.addEventListener('submit', (event) => {
        event.preventDefault();

        const password = form.querySelector('#passwordPopup');
        alert(password.value);


        if (password.value === '' || password.value === null || password.value === undefined) {
            password.style.display = 'block';
            //scroll to the password input
            password.scrollIntoView();


            return;
        }

        
        form.submit();
    });
});