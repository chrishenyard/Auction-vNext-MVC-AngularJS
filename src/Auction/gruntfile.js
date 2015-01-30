/// <binding />
// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
    grunt.initConfig({
        bower: {
            install: {
                options: {
                    targetDir: "wwwroot/lib",
                    layout: "byComponent",
                    cleanTargetDir: false
                }
            }
        },

        concat: {
        	options: {
        		stripBanners: true
        	},
        	dist: {
        		src: ['wwwroot/lib/spin.js/js/spin.js', 'wwwroot/lib/ladda-boostrap-hakimel/js/ladda-forked.js', 'wwwroot/app/lib/date.js', 'wwwroot/app/services/auction.svc.js', 'wwwroot/app/auction.js'],
        		dest: 'wwwroot/app/release/app.js'
        	},
        	distCss: {
        		src: ['wwwroot/css/auction.css'],
        		dest: 'wwwroot/app/release/app.css'
        	}
        },

        uglify: {
        	options: {
        		compress: {
        			drop_console: true
        		}
        	},
        	dist: {
        		src: ['<%= concat.dist.dest %>'],
        		dest: 'wwwroot/app/release/app.min.js'
        	}
        },

        cssmin: {
        	dist: {
        		src: ['<%= concat.distCss.dest %>'],
        		dest: 'wwwroot/app/release/app.min.css'
        	}
        }
    });

    /* grunt.registerTask('default', ['bower:install', 'concat', 'uglify', 'cssmin']); */
    grunt.registerTask('default', ['bower:install', 'concat', 'uglify']);

    grunt.loadNpmTasks("grunt-bower-task");
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
};