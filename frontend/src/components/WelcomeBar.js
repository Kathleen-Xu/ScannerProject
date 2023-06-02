import {Box, Typography} from "@mui/material";

export default function WelcomeBar() {
  return (
    <Box
      sx={{
        width: "100%" ,
        height: "175px",
        background: "linear-gradient(#c2c2c2, #f5f5f5)",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center"
      }}>
      <Typography
        variant="h4"
        gutterBottom

      >
        欢迎使用 Scanner
      </Typography>
      <Typography variant="subtitle1" gutterBottom>
        支持控件从WPF框架向Avalonia迁移与转换规则自定义
      </Typography>
    </Box>
  );
}