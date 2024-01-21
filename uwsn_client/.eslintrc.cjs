module.exports = {
    'react/no-unknown-property': ['off', { ignore: ['JSX'] }],
    extends: [
        'eslint:recommended',
        'plugin:react/recommended',
        'plugin:react/jsx-runtime',
        '@electron-toolkit/eslint-config-ts/recommended'
    ]
}
