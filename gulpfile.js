/// <binding ProjectOpened='watchTsAndViews_DEVELOPMENT' />
const gulpTs = require('@corprio/gulp-typescript');

//deletes everything inside wwwroot/js/views folder
exports.cleanJsViews = gulpTs.cleanJsViews;

/** process typescript files to transpile to .js files, and add declarations to ts from view
then keep watching for changes */
exports.processTs_DEVELOPMENT = gulpTs.processTs_DEVELOPMENT;
/** process typescript files to transpile to .js and .min.js files, and add declarations to ts from view
then keep watching for changes */
exports.processTs_PRODUCTION = gulpTs.processTs_PRODUCTION;

/** cleans www/js/views and repopulate with transpiled .js, refresh declarations in ts,
keeps watching for changes in views and ts to update */
exports.refreshAll_DEVELOPMENT = gulpTs.refreshAll_DEVELOPMENT;
/** cleans www/js/views and repopulate with transpiled .js and .min.js, refresh declarations in ts,
keeps watching for changes in views and ts to update */
exports.refreshAll_PRODUCTION = gulpTs.refreshAll_PRODUCTION;

/** watch for changes in typescript files and their views to transpile to .js or generate declaration on demand */
exports.watchTsAndViews_DEVELOPMENT = gulpTs.watchTsAndViews_DEVELOPMENT;
/** watch for changes in typescript files and their views to transpile to .js and .min.js or generate declaration on demand */
exports.watchTsAndViews_PRODUCTION = gulpTs.watchTsAndViews_PRODUCTION;