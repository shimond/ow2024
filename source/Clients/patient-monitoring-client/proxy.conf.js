module.exports = {
  "/api": {
    ws:true,
    target:
      process.env["services__webclientbffgateway__https__0"],
    secure: process.env["NODE_ENV"] !== "development",
    pathRewrite: {
      "^/api": "",
    },
  },
};