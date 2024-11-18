// site.js

// Navbar toggle için örnek bir işlev
document.addEventListener("DOMContentLoaded", function () {
    const navToggle = document.querySelector(".navbar-toggler");
    navToggle.addEventListener("click", function () {
        document.querySelector(".navbar-collapse").classList.toggle("show");
    });
});
