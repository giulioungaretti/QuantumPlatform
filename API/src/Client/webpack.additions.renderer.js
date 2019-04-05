var path = require("path");
var HtmlWebpackPlugin = require("html-webpack-plugin");

function resolve(filePath) {
  return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}

module.exports = {
  entry: [
    resolve("src/Renderer/Renderer.fsproj"),
    resolve("src/Renderer/style.sass")
  ],
  plugins: [
    new HtmlWebpackPlugin({
      filename: "index.html",
      template: resolve(".src/Renderer/index.html")
    })
  ],
  output: {
    filename: "renderer.js"
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader"
        }
      }
    ]
  }
};
