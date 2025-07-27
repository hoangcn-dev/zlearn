const scrollTo = (selector) => {
    $('html, body').animate({
        scrollTop: $(selector).offset().top - 50
    }, 500);
}

function showLoading() {
    $('#loader-container').show();
}

function hideLoading() {
    $('#loader-container').hide();
}

function getData(url, callback) {
    showLoading();
    $.ajax({
        url: url,
        method: "GET",
        xhrFields: { withCredentials: true },
        success: res => {
            hideLoading();
            if (res.succeeded) {
                callback(res.data);
            } else {
                console.error(res.message);
            }
        },
        error: error => {
            if (error.status === 403) {
                removeSessionData();
                window.location.href = "/forbidden";
            }
            hideLoading();
            console.error(error);
        }
    });
}

function saveSessionData() {
    $.ajax({
        url: "/auth/session-data",
        xhrFields: { withCredentials: true },
        method: "GET",
        success: res => {
            sessionStorage.setItem('userId', res.data.id);
            sessionStorage.setItem('userName', res.data.userName);
            sessionStorage.setItem('firstName', res.data.firstName);
            sessionStorage.setItem('lastName', res.data.lastName);
            sessionStorage.setItem('imagePath', res.data.imagePath);
            sessionStorage.setItem('roles', res.data.roles.join());

            $('#btnLogin').hide();
            $('#avt').attr('src', res.data.imagePath);
            $('#userName').text(sessionStorage.getItem('lastName') + " " + sessionStorage.getItem('firstName'));
            $('#roles').text(res.data.roles.join());
            $('#loggedInContainer').removeClass('d-none');
        },
        error: error => console.log(error)
    });
}

function getSessionData() {
    if (sessionStorage.getItem('userId')) {
        $('#avt').attr('src', sessionStorage.getItem('imagePath'));
        $('#userName').text(sessionStorage.getItem('lastName') + " " + sessionStorage.getItem('firstName'));
        $('#roles').text(sessionStorage.getItem('roles'));
        $('#btnLogin').hide();
        $('#loggedInContainer').removeClass('d-none');
        return true;
    }
    return false;
}

function removeSessionData() {
    sessionStorage.removeItem('userId');
    sessionStorage.removeItem('imagePath');
    sessionStorage.removeItem('userName');
    sessionStorage.removeItem('roles');
}
