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

CI 构建需要一个有效的 Unity 许可证。本项目采用**手动激活 .ulf 文件**的方式（不依赖账号密码）：

1. 打开 **Unity Hub → Preferences（齿轮）→ Licenses → Add → Get a free personal license**。
2. 添加后，若提示激活方式，选择 **Manual activation（手动激活）**，点击 **Save license request** 生成 `.alf` 文件。
3. 浏览器打开 <https://license.unity3d.com/manual>，上传刚才的 `.alf` 文件，许可证类型选 **Unity Personal Edition**，点击 Next。
4. 下载生成的 `.ulf` 文件（如 `Unity_lic.ulf`），用文本编辑器打开并**复制全部内容**。
5. 在 GitHub 仓库 **Settings → Secrets and variables → Actions → New repository secret**：
   - **Name**: `UNITY_LICENSE`
   - **Secret**: 粘贴 `.ulf` 文件的完整内容

> 注意：`.ulf` 内容含 XML 标签，需完整粘贴（包括 `<root> ... </root>`）。
> 如果本地已经激活过个人许可证，也可以直接把 `C:\ProgramData\Unity\Unity_lic.ulf`（Windows）或 `/Library/Application Support/Unity/Unity_lic.ulf`（macOS）的内容复制进去。

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
