const path = require("path");
const CopyWebpackPlugin = require("copy-webpack-plugin");
const pkg = require("./package.json");

module.exports = (env, args) => ({
    target: "web",
    mode: "production",
    resolve: { extensions: [".ts", ".js"] },
    devtool: "eval",
    module: {
        rules: [{ test: /\.ts?$/, loader: "ts-loader" }],
    },
    entry: "./src/index.ts",
    output: {
        filename: `index.js`,
        path: path.resolve(__dirname, "dist"),
        globalObject: "this",
        library: pkg.name,
        libraryTarget: "umd",
        umdNamedDefine: true,
    },
    plugins: [
        new CopyWebpackPlugin({
            patterns: [
                {
                    from: "static",
                },
            ],
        }),
    ],
});
