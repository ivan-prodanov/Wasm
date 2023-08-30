const path = require("path");
const webpack = require("webpack");
const pkg = require('./package.json');

const pkgName = "wasm-sdk";

module.exports = (env, args) => ({
  target: "web",
  mode: "production",
  resolve: { extensions: [".ts", ".js"] },
  devtool: "source-map",
  module: {
    rules: [{ test: /\.ts?$/, loader: "ts-loader" }],
  },
  entry: "./src/index.ts",
  output: {
    filename: `${pkgName}.js`,
    path: path.resolve(__dirname, "dist"),
    globalObject: "this",
    library: pkg.name,
    libraryTarget: "umd",
    umdNamedDefine: true,
  },
});
