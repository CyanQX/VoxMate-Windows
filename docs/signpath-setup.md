# SignPath Foundation 签名接入

当前 [v0.1.0 安装器](https://github.com/CyanQX/VoxMate-Windows/releases/tag/v0.1.0) 未签名。SignPath Foundation 的免费签名申请已提交，正在等待审核。只有 SignPath Foundation 批准项目、配置签名证书和项目后，才能通过本仓库的 GitHub Actions 生成带受信任签名的安装器。申请和审批结果由 SignPath 决定。

## 已准备的文件

- [代码签名政策](code-signing-policy.md)：公开签名范围、项目角色及隐私说明。
- [安装器构建工作流](../.github/workflows/package.yml)：从 GitHub 托管的 Windows runner 构建安装器，先将原始 EXE 保存为 Actions artifact，再按需提交 SignPath。
- [Artifact Configuration](../signing/signpath-artifact-configuration.xml)：只允许签署 `VoxMate-Setup-${version}-win-x64.exe`，并限制产品名、产品版本、文件版本和公司名。

## 获批后的配置

配置前，维护者应按 [GitHub 官方步骤](https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/configuring-two-factor-authentication)为 GitHub 启用双重验证，并为 SignPath 账号启用多因素认证。

1. 按 SignPath 的获批通知激活组织与 VoxMate 项目，添加 GitHub.com Trusted Build System。若签名政策需要 GitHub 审计日志或源码策略验证，再按 SignPath 的要求安装 SignPath GitHub App，仅授予此仓库所需权限。
2. 在 SignPath 项目中建立 Artifact Configuration，将 `signing/signpath-artifact-configuration.xml` 的内容粘贴进去，保存其 slug。为项目配置 SignPath Foundation 证书和要求每次签名人工批准的 Signing Policy。配置由 SignPath 提供的组织与项目值，不要凭空填写。
3. GitHub 仓库 Settings → Secrets and variables → Actions：设置 secret `SIGNPATH_API_TOKEN`；设置 variables `SIGNPATH_ORGANIZATION_ID`、`SIGNPATH_PROJECT_SLUG`、`SIGNPATH_SIGNING_POLICY_SLUG`、`SIGNPATH_ARTIFACT_CONFIGURATION_SLUG`。令牌只授予提交签名请求所需权限，不写入仓库。
4. GitHub Actions → **Build Windows installer** → Run workflow，选 `main`，把 `sign_with_signpath` 设为 `true`。SignPath 的审批人核对源代码、构建记录与版本后，手动批准该次请求。GitHub Action 随后核验 Authenticode 状态、发布者和版本，再生成签名后文件的 SHA-256。
5. 从该工作流的 `VoxMate-signed-installer` artifact 取得文件，复核 Authenticode 签名与 SHA-256 后，再作为对应版本的 GitHub Release 附件发布，并在发布页明确标注签名状态及代码签名政策。不要用未签名文件的校验值验证签名后的文件。

工作流默认 `sign_with_signpath=false`，所以可在尚未获批时手动验证构建，但产物仍是未签名的。源码仓库的 `main` 不保存 EXE、DLL、模型或签名密钥。

SignPath Foundation 要求所有维护者在 GitHub 和 SignPath 启用多因素认证；对可下载并执行的软件，还要求可核实的项目声誉。新项目的申请可能被拒绝，不能以虚构下载量、用户反馈或媒体报道代替证据。

参考：[SignPath Foundation 条件](https://signpath.org/terms.html)、[GitHub Trusted Build System](https://docs.signpath.io/trusted-build-systems/github)、[Artifact Configuration 语法](https://docs.signpath.io/artifact-configuration/syntax)。
