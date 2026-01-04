# Unity WebGL 部署指南

## 问题背景

Unity WebGL 构建默认使用 gzip 压缩 (`.gz` 文件)，但普通的 http-server 无法正确设置 `Content-Encoding: gzip` 响应头，导致浏览器无法自动解压文件，出现以下错误：

```text
Failed to parse binary data file Build/WebApp.data.gz because it is still gzip-compressed.
It should have been uncompressed by the browser, but it was unable to do so since 
the web server provided the compressed content without specifying the HTTP Response 
Header "Content-Encoding: gzip"
```

## 解决方案

创建自定义的 Express 服务器 (`webgl-server.js`)，自动为 `.gz` 文件添加正确的响应头。

---

## 环境要求

- **Node.js**: 18.0 或更高版本
- **npm**: Node.js 自带的包管理器

### 检查 Node.js 版本

```bash
node --version
```

如果未安装或版本过低，请从 [Node.js 官网](https://nodejs.org/) 下载安装。

---

## 首次安装步骤

### 1. 安装项目依赖

在项目根目录下执行：

```bash
npm install
```

这会根据 `package.json` 自动安装 `express` 依赖包。

---

## 启动服务器

### 方法 1: 使用 Node.js 直接运行

```bash
node webgl-server.js
```

### 方法 2: 添加到 package.json scripts (推荐)

在 `package.json` 中添加：

```json
{
  "scripts": {
    "start": "node webgl-server.js"
  }
}
```

然后使用：

```bash
npm start
```

### 启动成功输出

```bash
Unity WebGL Server running at:
  http://localhost:58497
  http://127.0.0.1:58497
  http://192.168.2.121:58497

Press Ctrl+C to stop
```

---

## 访问应用

启动服务器后，在浏览器中访问：

- **本地访问**: <http://localhost:58497>
- **局域网访问**: <http://192.168.2.121:58497> (IP 可能不同)

---

## 服务器配置说明

`webgl-server.js` 的主要功能：

### 1. 正确的 Content-Encoding 头

```javascript
// 为 .gz 文件自动添加 Content-Encoding: gzip
if (req.url.endsWith('.gz')) {
  res.set('Content-Encoding', 'gzip');
}
```

### 2. 设置正确的 Content-Type

- `.data.gz` → `application/octet-stream`
- `.js.gz` → `application/javascript`
- `.wasm.gz` → `application/wasm`

### 3. 启用 CORS

```javascript
res.header('Access-Control-Allow-Origin', '*');
```

### 4. 禁用缓存 (开发环境)

```javascript
res.header('Cache-Control', 'no-cache, no-store, must-revalidate');
```

---

## 端口配置

默认端口: **58497**

如需修改，编辑 `webgl-server.js`:

```javascript
const PORT = 58497; // 修改为你需要的端口
```

---

## 常见问题

### Q1: 提示 "Cannot find module 'express'"

**解决方法:**

```bash
npm install
```

### Q2: 端口被占用

**错误信息:**

```bash
Error: listen EADDRINUSE: address already in use :::58497
```

**解决方法:**

1. 修改 `webgl-server.js` 中的端口号
2. 或终止占用端口的进程:

   ```bash
   # macOS/Linux
   lsof -ti:58497 | xargs kill -9
   
   # Windows
   netstat -ano | findstr :58497
   taskkill /PID <进程ID> /F
   ```

### Q3: 浏览器显示 "This site can't be reached"

**检查项:**

- 服务器是否正在运行
- 端口号是否正确
- 防火墙是否阻止了连接

---

## 生产环境部署建议

### 1. 使用 PM2 进程管理器

```bash
# 全局安装 PM2
npm install -g pm2

# 启动服务器
pm2 start webgl-server.js --name "unity-webgl"

# 开机自启动
pm2 startup
pm2 save

# 查看日志
pm2 logs unity-webgl

# 重启服务
pm2 restart unity-webgl
```

### 2. 启用 HTTPS

推荐使用 Nginx 反向代理:

```nginx
server {
    listen 443 ssl;
    server_name yourdomain.com;
    
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    
    location / {
        proxy_pass http://localhost:58497;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 3. 启用缓存 (生产环境)

修改 `webgl-server.js`，移除或注释掉禁用缓存的代码：

```javascript
// 生产环境启用缓存
app.use((req, res, next) => {
  res.header('Cache-Control', 'public, max-age=31536000'); // 1年
  next();
});
```

---

## Unity 构建设置

确保 Unity 项目使用 Gzip 压缩:

1. **File → Build Settings → WebGL**
2. **Player Settings → Publishing Settings**
3. **Compression Format**: 选择 **Gzip**

---

## 文件说明

```text
Unity6Demo/
├── webgl-server.js       # 自定义服务器脚本
├── package.json          # Node.js 依赖配置
├── package-lock.json     # 依赖版本锁定文件 (自动生成)
├── node_modules/         # npm 安装的依赖包 (已在 .gitignore 中)
└── WebApp/               # Unity WebGL 构建输出目录
    ├── index.html
    └── Build/
        ├── WebApp.data.gz
        ├── WebApp.framework.js.gz
        ├── WebApp.loader.js
        └── WebApp.wasm.gz
```

---

## Git 版本控制

`.gitignore` 已配置忽略以下 Node.js 相关文件:

```gitignore
# Node.js 相关
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
package-lock.json
```

**需要提交的文件:**

- ✅ `webgl-server.js`
- ✅ `package.json`
- ❌ `node_modules/` (不提交)
- ❌ `package-lock.json` (不提交)

---

## 相关链接

- [Node.js 官网](https://nodejs.org/)
- [Express 文档](https://expressjs.com/)
- [Unity WebGL 文档](https://docs.unity3d.com/Manual/webgl.html)
- [PM2 进程管理器](https://pm2.keymetrics.io/)

---

## 更新日志

### 2025-11-24

- ✅ 创建自定义 Express 服务器
- ✅ 解决 gzip 压缩文件加载问题
- ✅ 配置 CORS 支持
- ✅ 添加 .gitignore 规则

---

## 技术支持

如遇到问题，请检查:

1. Node.js 版本是否符合要求
2. 依赖是否正确安装 (`npm install`)
3. 端口是否被占用
4. 浏览器控制台是否有错误信息

更多问题请联系项目维护者。
