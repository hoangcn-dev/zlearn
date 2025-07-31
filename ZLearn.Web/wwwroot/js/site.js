const scrollTo = (selector) => {
    window.scrollTo({
        top: $(selector).offset().top - 50,
        behavior: 'smooth'
    });
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

function postJsonData(url, data, callback, isShowMess = false) {
    showLoading();
    $.ajax({
        url: url,
        method: "POST",
        xhrFields: { withCredentials: true },
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(data),
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

function postFormData(url, formData, callback, isShowMess = false) {
    showLoading();
    $.ajax({
        url: url,
        method: "POST",
        xhrFields: { withCredentials: true },
        contentType: false,
        processData: false,
        data: formData,
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

            showMess(`Đăng nhập thành công, xin chào ${res.data.firstName}!`, true);
        },
        error: error => console.log(error)
    });
}

function showConfirm(msg, callback) {
    const confirmDialog = $(`
        <div id="confirm-dialog" class="vh-100 vw-100 position-fixed top-0 start-0 bg-secondary bg-opacity-50">
            <div class="position-absolute start-50 translate-middle-x bg-white text-dark p-2 px-4" style="margin-top: 100px; z-index: 1; max-width: 420px;">
                <div class="d-flex flex-column align-items-center">
                    <div class="btn-close position-absolute end-0 top-0 m-1 opacity-hover"></div>
                    <div class="text-center">${msg}</div>
                    <div class="d-flex align-items-center justify-content-center mt-2">
                        <button class="confirm fw-light border text-white shadow-sm p-1 px-2 opacity-hover" style="background-color: var(--main-color); width: 100px;">
                            <i class="fa-solid fa-check me-1"></i>Xác nhận
                        </button>
                        <button class="cancel ms-2 fw-light border bg-white shadow-sm p-1 px-2 highlight-hover" style="width: 100px;">
                            <i class="fa-solid fa-xmark me-1" style="color: var(--error-color)"></i>Hủy
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `);
    confirmDialog.find('.btn-close').click(() => confirmDialog.remove());
    confirmDialog.find('.cancel').click(() => confirmDialog.remove());
    confirmDialog.find('.confirm').click(() => {
        callback();
        confirmDialog.remove();
    });
    $('body').append(confirmDialog);
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
    sessionStorage.removeItem('firstName');
    sessionStorage.removeItem('lastName');

    $.ajax({
        url: "/auth/sign-out",
        xhrFields: { withCredentials: true },
        method: "POST",
        success: res => {
            $('#loggedInContainer').addClass('d-none');
            $('#btnLogin').show();
            showMess(`Đăng xuất thành công!`, true);
        },
        error: error => console.log(error)
    })
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

function updateQueryParam(key, value) {
    const url = new URL(window.location);
    url.searchParams.set(key, value);
    window.history.replaceState({}, '', url);
}

async function saveFiles(files) {
    var formData = new FormData();
    for (var i = 0; i < files.length; i++) {
        formData.append(`files[${i}].data`, files[i]);
        formData.append(`files[${i}].name`, files[i].name);
    }
    return new Promise(resolve => {
        showLoading();
        $.ajax({
            url: '/files',
            method: "POST",
            xhrFields: { withCredentials: true },
            contentType: false,
            processData: false,
            data: formData,
            success: res => {
                hideLoading();
                if (res.succeeded) {
                    resolve(res);
                } else {
                    console.error(res.message);
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
    });
}






