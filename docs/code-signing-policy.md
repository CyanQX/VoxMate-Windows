# Code signing policy / 代码签名政策

VoxMate 的源码和构建脚本由项目维护者 [CyanQX](https://github.com/CyanQX) 维护。维护者负责审核外部贡献、核对发布构建来源，并批准每次正式签名请求。项目的[隐私说明](privacy.md)描述了本地录音、文本与历史记录的处理方式。

计划申请的免费服务：**Free code signing provided by SignPath.io, certificate by SignPath Foundation**。目前尚未获得 SignPath Foundation 批准，也没有受其证书签署的 VoxMate 发布包。申请获批前的安装器会明确标为未签名。

签名仅用于由本项目公开源码和构建脚本产生的正式发布文件。发布文件应从受保护的 GitHub 仓库构建、保留可核对的版本与产品元数据，并在签名后重新计算 SHA-256。项目不会使用本签名身份签署第三方的 whisper.cpp、llama.cpp 或其它上游二进制文件。

项目目前由 [CyanQX](https://github.com/CyanQX) 担任提交者、外部贡献审核者和发布签名批准者。如有其他维护者加入，角色和审核规则会在公开仓库更新。仓库和签名服务账号都应启用多因素认证；私钥、令牌和密码不得写入仓库。
