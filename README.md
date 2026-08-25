# unity-ci-test

用 [game-ci](https://game.ci/) 在 GitHub Actions 上构建 **Unity 6（6000.3.14f1）Android 版本**的空项目，仅用于测试 CI 构建流程。

## 项目结构

```
unity-ci-test/
├── Assets/
│   └── Editor/
│       └── BuildScript.cs          # 构建入口 BuildCommand.Build（空项目会自动创建空场景）
├── Packages/
│   └── manifest.json               # 仅内置模块，不依赖第三方包
├── ProjectSettings/
│   └── ProjectVersion.txt          # Unity 6000.3.14f1
├── .github/workflows/
│   └── build-android.yml           # game-ci/unity-builder@v4 构建 Android
└── .gitignore
```

## 一次性配置：Unity 许可证（必须）

Unity 6 的个人许可证（Personal）**没有序列号，不能用账号密码在 CI 里自动激活**，必须提供一个 `.ulf` 许可证文件。该文件绑定生成它的机器，所以要在 GitHub runner 上生成 `.alf` 再手动激活。

### 步骤 1：在 CI 上生成激活请求文件（.alf）

1. 到 **Actions → Acquire activation file → Run workflow** 手动运行一次；
2. 运行完成后进入该 run 的 **Artifacts** 区域，下载生成的 `.alf` 文件（如 `Unity_v6000.3.14f1.alf`）。

### 步骤 2：激活得到 .ulf

1. 浏览器打开 <https://license.unity3d.com/manual>，登录你的**国际版 Unity ID**；
2. 上传刚才的 `.alf` 文件，许可证类型选 **Unity Personal Edition**，点 Next；
3. 下载生成的 `.ulf` 文件。

### 步骤 3：配置 secret

在 GitHub 仓库 **Settings → Secrets and variables → Actions → New repository secret**：

- **Name**: `UNITY_LICENSE`
- **Secret**: 用文本编辑器打开 `.ulf`，粘贴**完整内容**（含 `<?xml ...?>` 和 `<root> ... </root>`）

> 说明：`UNITY_EMAIL` / `UNITY_PASSWORD` 只对带序列号的 Pro/Plus 许可证有用；个人许可证只需 `UNITY_LICENSE`。

## 触发构建

配置好 `UNITY_LICENSE` secret 后：

- **推送代码**到 `main` 分支会自动触发；
- 也可以到 **Actions 标签页 → Build Android → Run workflow** 手动触发。

构建产物（`build/Android/*.apk`）会作为 workflow artifact 保留 14 天，在对应 run 的 **Artifacts** 区域下载。

## 本地验证（可选）

在本地用 Unity 打开项目目录，或用命令行手动构建：

```bash
Unity -batchmode -quit -projectPath . \
  -buildTarget Android \
  -customBuildTarget Android \
  -customBuildPath build/Android \
  -customBuildName unity-ci-test \
  -executeMethod BuildCommand.Build
```

（`Unity` 需替换为你本机 Unity 编辑器的可执行文件路径。）

## 关键说明

- **Unity 版本**：`6000.3.14f1`。若 game-ci 镜像不存在该版本，将 `ProjectVersion.txt` 和 workflow 里的 `unityVersion` 一并改成一个可用的 `6000.x` 版本。
- **Scripting Backend**：Unity 6 的 Android 默认使用 IL2CPP，game-ci 镜像已内置 Android SDK/NDK/JDK，无需额外配置。
- **签名**：当前是 debug 构建（未配置 keystore），APK 可直接安装用于流程验证。
