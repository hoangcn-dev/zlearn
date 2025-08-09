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

async function getData(url, callback) {
    showLoading();
    const res = await callApi(url, 'GET');
    hideLoading();
    callback(res);
}

async function postJsonData(url, data, callback) {
    showLoading();
    //$.ajax({
    //    url: url,
    //    method: "POST",
    //    xhrFields: { withCredentials: true },
    //    contentType: "application/json; charset=utf-8",
    //    data: JSON.stringify(data),
    //    success: res => {
    //        hideLoading();
    //        if (res.succeeded) {
    //            callback(res.data);
    //        } else {
    //            console.error(res.message);
    //        }

    //        if (isShowMess) {
    //            showMess(res.message, res.succeeded);
    //        }
    //    },
    //    error: error => {
    //        if (error.status === 403) {
    //            removeSessionData();
    //            window.location.href = "/forbidden";
    //        }
    //        else if (error.status == 401) {
    //            showLoginDialog();

    //        }
    //        hideLoading();
    //        console.error(error);
    //    }
    //});
    const res = await callApi(url, 'POST', 'application/json; charset=utf-8', JSON.stringify(data));
    debugger
    hideLoading();
    callback(res);
}

async function pustJsonData(url, data, callback) {
    showLoading();
    //$.ajax({
    //    url: url,
    //    method: "PUT",
    //    xhrFields: { withCredentials: true },
    //    contentType: "application/json; charset=utf-8",
    //    data: JSON.stringify(data),
    //    success: res => {
    //        hideLoading();
    //        callback(res.data);
    //    },
    //    error: error => {
    //        if (error.status === 403) {
    //            removeSessionData();
    //            window.location.href = "/forbidden";
    //        }
    //        else if (error.status == 401) {
    //            $('#login-dialog').show()
    //        }
    //        hideLoading();
    //        showMess(error.responseJSON.message, false);
    //        console.error(error);
    //    }
    //});
    const res = await callApi(url, 'PUT', 'application/json; charset=utf-8', JSON.stringify(data));
    debugger
    hideLoading();
    callback(res);
}

async function callApi(url, method, contentType = 'application/json; charset=utf-8', data = {}) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: url,
            method: method,
            xhrFields: { withCredentials: true },
            contentType: contentType,
            data: data,
            success: res => {
                resolve(res);
            },
            error: error => {
                if (error.status === 403) {
                    removeSessionData();
                    window.location.href = "/forbidden";
                }
                else if (error.status === 401) {
                    showLoginDialog();
                }
                resolve(error.responseJSON);
                showMess(error.responseJSON.message, false);
                console.error(error);
            }
        })
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
        <div id="confirm-dialog" class="vh-100 vw-100 position-fixed top-0 start-0 bg-secondary bg-opacity-50" style="z-index: 100;">
            <div class="position-absolute start-50 translate-middle-x bg-white text-dark p-2 px-4" style="margin-top: 100px; max-width: 420px;">
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
        <div id="${uniqueId}" class="position-fixed start-50 top-0 translate-middle-x shadow-sm" style="margin-top: 40px; z-index: 101;">
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

function getMediaTypeFromUrl(url) {
    const extension = (url.match(/\.([0-9a-z]+)$/i) || [])[1]?.toLowerCase();
    if (!extension) return 'unknown';

    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp'];
    const videoExtensions = ['mp4', 'webm', 'ogv'];
    const audioExtensions = ['mp3', 'wav', 'ogg'];

    if (imageExtensions.includes(extension)) return 'image';
    if (videoExtensions.includes(extension)) return 'video';
    if (audioExtensions.includes(extension)) return 'audio';
    return 'unknown';
}


function previewMediaFile(url) {
    const dialog = $(`
        <div class="vh-100 vw-100 position-fixed top-0 start-0 bg-secondary bg-opacity-50" style="z-index: 100">
            <i class="fa-solid fa-xmark fs-1 text-white position-absolute start-0 top-0 m-3 opacity-hover"></i>
            <div class="media-container"></div>
        </div>
    `);
    dialog.find('i').click(() => dialog.remove());
    const mediaType = getMediaTypeFromUrl(url);
    if (url.includes('image')) {
        dialog.find('.media-container').append(`
            <img src="${url}" alt="" class="position-absolute start-50 top-50 translate-middle bg-white object-fit-contain" width="600" height="500">
        `);
    } else if (url.includes('video')) {
        dialog.find('.media-container').append(`
            <video controls class="position-absolute start-50 top-50 translate-middle bg-white object-fit-contain" width="600" height="500">
                <source src="${url}">
            </video>
        `);
    } else if (url.includes('audio')) {
        dialog.find('.media-container').append(`
            <audio controls class="position-absolute start-50 top-50 translate-middle">
                <source src="${url}" type="audio/mpeg">
                Your browser does not support the audio element.
            </audio>
        `);
    }
    $('body').append(dialog);
}

function showLoginDialog() {
    const dialog = $(`
        <div class="vw-100 vh-100 bg-secondary position-absolute top-0 start-0 bg-opacity-50" style="z-index: 3;">
            <div class="text-center position-absolute bg-white translate-middle-x start-50 top-0 rounded-1 p-4" style="width: 400px; margin-top: 100px; box-shadow: 0px 0px 10px #bcbcbc;">
                <button class="btn-close position-absolute end-0 top-0 m-1" onclick="document.getElementById('login-dialog').hidden = true"></button>
                <h3 class="text-center mb-1 fw-bold">ZLEARN</h3>
                <p class="fw-light">Vui lòng đăng nhập để tiếp tục</p>
                <img class="mb-4" src="/images/logo-light-blue.png" alt="" asp-append-version="true" width="100">
                <a href="/auth/sign-in/google?returnUrl=${encodeURIComponent(window.location.href)}" class="btn btn-dark d-flex justify-content-center w-100 align-items-center border-0 opacity-hover" style="background-color: var(--main-color);">
                    <div class="fw-light align-content-center">Đăng nhập với Google</div>
                    <svg class="ms-1" xmlns="http://www.w3.org/2000/svg" x="0px" y="0px" width="30" height="30" viewBox="0 0 48 48"><path fill="#FFC107" d="M43.611,20.083H42V20H24v8h11.303c-1.649,4.657-6.08,8-11.303,8c-6.627,0-12-5.373-12-12c0-6.627,5.373-12,12-12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C12.955,4,4,12.955,4,24c0,11.045,8.955,20,20,20c11.045,0,20-8.955,20-20C44,22.659,43.862,21.35,43.611,20.083z"></path><path fill="#FF3D00" d="M6.306,14.691l6.571,4.819C14.655,15.108,18.961,12,24,12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C16.318,4,9.656,8.337,6.306,14.691z"></path><path fill="#4CAF50" d="M24,44c5.166,0,9.86-1.977,13.409-5.192l-6.19-5.238C29.211,35.091,26.715,36,24,36c-5.202,0-9.619-3.317-11.283-7.946l-6.522,5.025C9.505,39.556,16.227,44,24,44z"></path><path fill="#1976D2" d="M43.611,20.083H42V20H24v8h11.303c-0.792,2.237-2.231,4.166-4.087,5.571c0.001-0.001,0.002-0.001,0.003-0.002l6.19,5.238C36.971,39.205,44,34,44,24C44,22.659,43.862,21.35,43.611,20.083z"></path></svg>
                </a>
            </div>
        </div>
    `);
    dialog.find('.btn-close').click(() => dialog.remove());
    $('body').append(dialog);
}




