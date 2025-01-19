function login() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            username: username,
            password: password
        })
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(data => Promise.reject(data));
        }
        return response.json();
    })
    .then(data => {
        // Успешная авторизация
        Swal.fire({
            icon: 'success',
            title: 'Успешно!',
            text: 'Вы успешно вошли в систему',
            confirmButtonColor: '#3085d6'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = '/';
            }
        });
    })
    .catch(error => {
        // Ошибка авторизации
        Swal.fire({
            icon: 'error',
            title: 'Ошибка',
            text: 'Неверное имя пользователя или пароль',
            confirmButtonColor: '#3085d6'
        });
    });
} 