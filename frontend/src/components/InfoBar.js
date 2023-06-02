import {Box, Button, Chip, Grid, Typography} from "@mui/material";
import PropTypes from "prop-types";
import * as React from "react";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";

export default function InfoBar(props) {
  const { canMigrate, onMigrate, onCopy } = props;
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "end",
        borderBottom: "solid #c2c2c2 1px",
        padding: "0px 15px 0px 15px"
      }}
    >
      <Typography variant="button" display="block" gutterBottom sx={{ marginRight: "10px" }}>
        WPF
      </Typography>
      <Button
        color="primary"
        component="label"
        disabled={!canMigrate}
        endIcon={<ArrowForwardIcon/>}
        onClick={onMigrate}
      >
        转换
      </Button>
      <Box sx={{ flex: "1" }}/>
      <Typography variant="button" display="block" gutterBottom sx={{ marginRight: "10px" }}>
        Avalonia
      </Typography>
      <Button
        color="primary"
        component="label"
        disabled={!canMigrate}
        endIcon={<ContentCopyIcon/>}
        onClick={onCopy}
      >
        复制
      </Button>
    </Box>
  );
}

InfoBar.propTypes = {
  canMigrate: PropTypes.bool.isRequired,
  onMigrate: PropTypes.func.isRequired,
  onCopy: PropTypes.func.isRequired
};