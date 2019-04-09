var path = require("path");

var CopyWebpackPlugin = require("copy-webpack-plugin");
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
      template: resolve("./src/Renderer/index.html")
    }),
    new CopyWebpackPlugin([
      { from: resolve("./src/Renderer/public"), to: "./src/Renderer/public" }
    ])
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
      },
      {
        test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
        use: ["file-loader"]
      }
    ]
  }
};
