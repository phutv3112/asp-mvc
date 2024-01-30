var gulp = require('gulp');
var sass = require('gulp-sass')(require('sass'));
var cssmin = require("gulp-cssmin");
const rename = require('gulp-rename');


gulp.task("default", function () {
    return gulp.src('assets/scss/site.scss')
        .pipe(sass().on('error', sass.logError))
        //.pipe(cssmin())
        .pipe(rename({
            //suffix: ".min"
        }))
        .pipe(gulp.dest('wwwroot/css/'))
})
gulp.task("watch", function () {
    gulp.watch('assets/scss/*.scss', gulp.series("default"))
})