const express = require('express');
const path = require('path');
const app = express();
const PORT = 58497;

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

// 禁用缓存
app.use((req, res, next) => {
  res.header('Cache-Control', 'no-cache, no-store, must-revalidate');
  next();
});

// 静态文件服务
app.use(express.static(path.join(__dirname, 'WebApp')));

app.listen(PORT, () => {
  console.log(`Unity WebGL Server running at:`);
  console.log(`  http://localhost:${PORT}`);
  console.log(`  http://127.0.0.1:${PORT}`);
  console.log(`  http://192.168.2.121:${PORT}`);
  console.log('\nPress Ctrl+C to stop');
});
