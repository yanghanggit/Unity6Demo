const express = require('express');
const path = require('path');
const fs = require('fs');
const app = express();
const PORT = 58497;
const WEBAPP_DIR = 'Web';

// 为 .gz 文件设置正确的 Content-Encoding 头
app.use((req, res, next) => {
  if (req.url.endsWith('.gz')) {
    res.set('Content-Encoding', 'gzip');
    
    // 根据原始文件类型设置 Content-Type
    if (req.url.endsWith('.data.gz')) {
      res.set('Content-Type', 'application/octet-stream');
    } else if (req.url.endsWith('.js.gz')) {
      res.set('Content-Type', 'application/javascript');
    } else if (req.url.endsWith('.wasm.gz')) {
      res.set('Content-Type', 'application/wasm');
    }
  }
  next();
});

// 启用 CORS
app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Range');
  next();
});

// 禁用缓存（完整版本）
app.use((req, res, next) => {
  res.header('Cache-Control', 'no-cache, no-store, must-revalidate, max-age=0');
  res.header('Pragma', 'no-cache');
  res.header('Expires', '0');
  next();
});

// 检查 Web 目录是否存在
const webappPath = path.join(__dirname, WEBAPP_DIR);
if (!fs.existsSync(webappPath)) {
  console.error(`❌ 错误: ${WEBAPP_DIR} 目录不存在: ${webappPath}`);
  console.error(`请先在 Unity 中构建 WebGL 项目到 ${WEBAPP_DIR} 目录`);
  process.exit(1);
}

// 静态文件服务
app.use(express.static(webappPath));

app.listen(PORT, () => {
  console.log(`Unity WebGL Server running at:`);
  console.log(`需要查看本机的地址，如果是局域网其他设备访问，请使用局域网IP地址，通过ifconfig或者ipconfig命令查看`);
  console.log(`  http://localhost:${PORT}`);
  console.log(`  http://127.0.0.1:${PORT}`);
  console.log(`  http://192.168.2.121:${PORT}`);
  console.log('\nPress Ctrl+C to stop');
});
