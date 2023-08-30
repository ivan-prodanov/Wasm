const { resolve } = require("path");
const pkg = require("./package.json");

const pkgName = "wasm-sdk-webpack-plugin";

module.exports = (env, args) => ({
  target: "node",
  mode: "production",
  resolve: { extensions: [".js"] },
  devtool: "source-map",
  entry: "./index.js",
  output: {
    filename: `${pkgName}.js`,
    path: resolve(__dirname, "dist"),
  },
});
