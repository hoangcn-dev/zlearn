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

function getData(url, callback, isShowMess = false) {
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

            if (isShowMess) {
                showMess(res.message, res.succeeded);
            }
        },
        error: error => {
            if (error.status === 403) {
                removeSessionData();
                window.location.href = "/forbidden";
            }
            else if (error.status == 401) {
                $('#login-dialog').show()
                
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

function showMess(msg, isSuccess = true) {
    const iconHtml = isSuccess
        ? '<i class="fa-solid fa-circle-check" style="color: rgb(0, 183, 43);"></i>'
        : '<i class="fa-solid fa-triangle-exclamation" style="color: red;"></i>';
    const uniqueId = `msg-${Date.now()}`;
    const div = $(`
        <div id="${uniqueId}" class="position-fixed start-50 top-0 translate-middle-x shadow-sm" style="margin-top: 40px; z-index: 1;">
            <div class="d-flex bg-white align-items-center px-2 py-1">
                ${iconHtml}
                <div class="mx-2" style="font-size: 14px;">${msg}</div>
                <i class="fa-solid fa-xmark ms-1 text-secondary opacity-hover close-btn"></i>
            </div>
        </div>
    `);
    $('body').append(div);
    div.hide();
    div.slideDown(500);
    setTimeout(() => {
        div.slideUp(500, () => div.remove());
    }, 3000);
    div.find('.close-btn').click(() => {
        div.slideUp(500, () => div.remove());
    });
}
