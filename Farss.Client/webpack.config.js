// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

var path = require("path");
var webpack = require("webpack");

module.exports = {
    mode: "development",
    entry: "./src/Farss.Client.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        contentBase: "./public",
        host: '0.0.0.0',
        port: 8080,
        proxy: {
            "/api": "http://localhost:5000"
        },
        hot: true,
        inline: true,
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "*",
            "Access-Control-Allow-Headers": "*"
        }
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        },
        {
            test: /\.css$/i,
            use: ['style-loader', 'css-loader'],
        }]
    },
    plugins: [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ]
}