var path = require("path");

function resolve(filePath) {
  return path.join(__dirname, filePath);
}

module.exports = {
  entry: [
    resolve("src/Renderer/Renderer.fsproj"),
    resolve("src/Renderer/style.sass")
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
