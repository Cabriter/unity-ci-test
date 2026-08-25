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

CI 构建需要一个**国际版 Unity 账号**，构建时用账号密码自动激活个人许可证（Personal License）。

1. 到 <https://unity.com> 注册/登录一个 Unity ID（注意：国际版账号与团结引擎 unity.cn 账号是两套体系，不通用）。
2. 在 GitHub 仓库 **Settings → Secrets and variables → Actions → New repository secret** 添加两个 secret：
   - **Name**: `UNITY_EMAIL`  →  **Secret**: 你的 Unity ID 邮箱
   - **Name**: `UNITY_PASSWORD`  →  **Secret**: 你的 Unity ID 密码

> 注意：
> - 个人许可证（Personal）有同时激活机器数上限（一般 2 台）；如需频繁构建，可进一步配合缓存激活结果。
> - 若账号密码激活失败，也欢迎改用 `UNITY_LICENSE`（手动激活的 `.ulf`）方式。

## 触发构建

配置好 `UNITY_EMAIL` / `UNITY_PASSWORD` 两个 secret 后：

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
