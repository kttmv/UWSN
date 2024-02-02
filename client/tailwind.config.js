/** @type {import('tailwindcss').Config} */
module.exports = {
    purge: [
        './src/renderer/**/*.js',
        './src/renderer/**/*.jsx',
        './src/renderer/**/*.ts',
        './src/renderer/**/*.tsx'
    ],
    darkMode: false, // or 'media' or 'class'
    theme: {
        extend: {}
    },
    variants: {},
    plugins: []
}
