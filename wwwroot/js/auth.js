function register() {
    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    // Проверяем длину пароля перед отправкой
    if (password.length < 6) {
        Swal.fire({
            icon: 'error',
            title: 'Ошибка валидации',
            text: 'Ошибка! Пароль должен содержать минимум 6 символов',
            confirmButtonColor: '#3085d6'
        });
        return;
    }

    fetch('/api/auth/register', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            username: username,
            email: email,
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
        // Успешная регистрация
        Swal.fire({
            icon: 'success',  // Используем success вместо error
            title: 'Успешно!',
            text: 'Регистрация прошла успешно',
            confirmButtonColor: '#3085d6'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = '/Home/Login';
            }
        });
    })
    .catch(error => {
        // Ошибка регистрации
        Swal.fire({
            icon: 'error',
            title: 'Ошибка',
            text: error.message || 'Произошла ошибка при регистрации',
            confirmButtonColor: '#3085d6'
        });
    });
} 