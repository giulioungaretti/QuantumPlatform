var path = require("path");
var webpack = require("webpack");
var CopyWebpackPlugin = require("copy-webpack-plugin");
var HtmlWebpackPlugin = require("html-webpack-plugin");
var MiniCssExtractPlugin = require("mini-css-extract-plugin");

function resolve(filePath) {
  return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}

var isProduction = !process.argv.find(
  v => v.indexOf("webpack-dev-server") !== -1
);

console.log(
  "Bundling electron renderer for " +
    (isProduction ? "production" : "development") +
    "..."
);

var commonPlugins = [
  new HtmlWebpackPlugin({
    filename: "index.html",
    template: resolve("./src/Renderer/index.html")
  }),
  new CopyWebpackPlugin([
    { from: resolve("./src/Renderer/public"), to: "./src/Renderer/public" }
  ])
];

module.exports = {
  entry: [
    resolve("src/Renderer/Renderer.fsproj"),
    resolve("src/Renderer/style.sass")
  ],
  plugins: isProduction
    ? commonPlugins.concat([
        new MiniCssExtractPlugin({ filename: "style.css" })
      ])
    : commonPlugins.concat([new webpack.HotModuleReplacementPlugin()]),
  resolve: {
    // See https://github.com/fable-compiler/Fable/issues/1490
    symlinks: false
  },
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
      },
      {
        test: /\.(sass|scss|css)$/,
        use: [
          isProduction ? MiniCssExtractPlugin.loader : "style-loader",
          "css-loader",
          {
            loader: "sass-loader",
            options: { implementation: require("sass") }
          }
        ]
      }
    ]
  }
};
