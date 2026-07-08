const express = require('express');
const path = require('path');
const fs = require('fs');
const os = require('os');
const app = express();
const PORT = 58497;
const WEBAPP_DIR = 'Builds/WebGL';

/**
 * 获取本机所有局域网 IPv4 地址（排除回环地址），方便打印出手机可访问的地址。
 */
function getLanIPs() {
  const results = [];
  const interfaces = os.networkInterfaces();
  for (const name of Object.keys(interfaces)) {
    for (const iface of interfaces[name]) {
      if (iface.family === 'IPv4' && !iface.internal) {
        results.push(iface.address);
      }
    }
  }
  return results;
}


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

app.listen(PORT, '0.0.0.0', () => {
  const lanIPs = getLanIPs();
  console.log(`Unity WebGL Server running at:`);
  console.log(`  http://localhost:${PORT}`);
  console.log(`  http://127.0.0.1:${PORT}`);
  if (lanIPs.length > 0) {
    console.log(`\n手机/其他局域网设备请访问：`);
    lanIPs.forEach((ip) => console.log(`  http://${ip}:${PORT}`));
  } else {
    console.log(`\n未检测到局域网 IP，请用 ifconfig（Mac/Linux）或 ipconfig（Windows）手动查看`);
  }
  console.log('\nPress Ctrl+C to stop');
});
