import * as React from "react";
import DriveFolderUploadIcon from "@mui/icons-material/DriveFolderUpload";
import FolderIcon from "@mui/icons-material/Folder";
import InsertDriveFileIcon from "@mui/icons-material/InsertDriveFile";
import FolderOpenIcon from "@mui/icons-material/FolderOpen";
import {TreeView} from "@mui/lab";
import {Box, Button, Typography} from "@mui/material";
import StyledTreeItem from "./StyledTreeItem";
import PropTypes from "prop-types";


export default function DirectoryViewer(props) {

  const { projectTree, onFileChange, onFileSelect } = props;

  return (
    <Box >
      <Box
        sx={{
          display: "flex",
          alignItems: "end",
          borderBottom: "solid #c2c2c2 1px",
          padding: "0 5px 0 15px"
        }}
      >
        <Typography variant="button" display="block" gutterBottom sx={{flex: "1"}}>
          项目文件目录
        </Typography>
        <Button color="primary" component="label" startIcon={<DriveFolderUploadIcon />}>
          上传
          <input hidden type="file" webkitdirectory="true" onChange={onFileChange}/>
        </Button>
      </Box>

      {projectTree !== null
      && <Box sx={{ padding: "3px 5px 3px 3px"}}>
        <TreeView
          aria-label="directory"
          defaultExpanded={['1']}
          defaultCollapseIcon={<FolderOpenIcon />}
          defaultExpandIcon={<FolderIcon />}
          defaultEndIcon={<InsertDriveFileIcon />}
          sx={{ height: "494px", overflowY: "auto"}}
        >
          <StyledTreeItem nodeId="1" file={projectTree} onSelectFile={onFileSelect}/>
        </TreeView>
      </Box>}
    </Box>
  );
}


DirectoryViewer.propTypes = {
  projectTree: PropTypes.object.isRequired,
  onFileChange: PropTypes.func.isRequired,
  onFileSelect: PropTypes.func.isRequired
};
