const scrollTo = (selector) => {
    $('html, body').animate({
        scrollTop: $(selector).offset().top - 50
    }, 900);
}
