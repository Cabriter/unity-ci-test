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

Unity 6 个人许可证的 `.ulf` 会绑定本机硬件（含 MAC 地址），不能直接用于 CI。正确做法是从本地激活的 `.ulf` 里**提取序列号**，再用序列号 + 账号密码在 CI 上激活。

### 步骤 1：本地激活国际版个人许可证

1. 从 <https://unity.com/download> 下载**国际版 Unity Hub**（不是团结引擎 unity.cn 的 Hub，账号不通用）；
2. 用国际版 Unity ID 登录；
3. **Preferences → Licenses → Add → Get a free personal license** 在线激活。

### 步骤 2：提取序列号（serial）

打开 `C:\ProgramData\Unity\Unity_lic.ulf`，找到 `<DeveloperData Value="..."/>`，把 `Value` 的 base64 内容解码，去掉开头 4 字节（`AQAA`）后就是序列号。

例如 `Value="AQAAAEY0LVE2VE4tNE00QS1KUFJNLUZYSEgtV1JXUQ=="` 解码后得到 `F4-Q6TN-4M4A-JPRM-FXHH-WRWQ`。

### 步骤 3：配置三个 secret

在 GitHub 仓库 **Settings → Secrets and variables → Actions → New repository secret** 添加：

- **Name**: `UNITY_SERIAL`  →  **Secret**: 上一步提取的序列号
- **Name**: `UNITY_EMAIL`  →  **Secret**: 你的国际版 Unity ID 邮箱
- **Name**: `UNITY_PASSWORD`  →  **Secret**: 你的 Unity ID 密码

> 说明：个人许可证虽然没有「购买」序列号，但 Unity 仍会为账号分配一个，藏在 `.ulf` 的 `DeveloperData` 里。CI 用它 + 账号密码在 runner 上重新激活。

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
