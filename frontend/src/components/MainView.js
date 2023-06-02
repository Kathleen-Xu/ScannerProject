import * as React from "react";
import {Grid, Button, Stack, Box} from "@mui/material";
import {useEffect, useState} from "react";
import axios from "axios";
import DirectoryViewer from "./DirectoryViewer";
import InfoBar from "./InfoBar";
import DiffViewer from "./DiffViewer";
import RuleManager from "./RuleManager";
import {instanceOf} from "prop-types";

axios.defaults.headers.post["Content-Type"] = "application/json";
const server = "http://localhost:11941/api";

export default function MainView() {

  const [fileList, setFileList] = useState([]);
  const [projectTree, setProjectTree] = useState(null);
  const [currentFile, setCurrentFile] = useState(null);
  const [currentType, setCurrentType] = useState("");
  const [wpfCode, setWpfCode] = useState("");
  const [avaCode, setAvaCode] = useState("");
  const [rules, setRules] = useState([]);

  function OnFileChange(e) {
    let files = e.target.files;
    let fileList = [];
    for (let i = 0; i < files.length; i++) {
      fileList.push(files[i]);
    }
    setFileList(fileList);
    // generate project folder
    let projFolder = {};
    fileList.forEach((file) => {
      let paths = file.webkitRelativePath.split("/");
      // init projFolder
      if (projFolder.name === undefined) {
        projFolder.name = paths[0];
        projFolder.childFolders = [];
        projFolder.childFiles = [];
      }
      // search in the folder directory step by step
      let pointer = projFolder;
      for (let i = 1; i < paths.length - 1; i++) {
        let isFound = false;
        for (let j = 0; j < pointer.childFolders.length; j++) {
          const tmp = pointer.childFolders[j];
          if (tmp.name === paths[i]) {
            pointer = tmp;
            isFound = true;
            break;
          }
        }
        if (!isFound) {
          let newFolder = {
            name: paths[i],
            childFolders: [],
            childFiles: []
          };
          pointer.childFolders.push(newFolder);
          pointer = newFolder;
        }
      }
      // add the new file to current folder
      pointer.childFiles.push({
        name: paths[paths.length-1],
        realFile: file
      });
    });
    // update to project tree
    setProjectTree(projFolder);
    setCurrentFile(null);
    setWpfCode("");
  }

  function selectFile(file) {
    setCurrentFile(file);
  }

  useEffect(() => {
    if (currentFile !== null) {
      let tmp = currentFile.name.split(".");
      let suffix = tmp[tmp.length - 1];
      setCurrentType(suffix);
      generateCode(currentFile, setWpfCode);
    }
    setWpfCode("");
    setAvaCode("");
  }, [currentFile]);

  useEffect(() => {
    requestPreRules();
  }, []);

  async function requestPreRules() {
    let res = await axios.get(`${server}/Migration/`);
    setRules(res.data.updates);console.log(res.data.updates.length);
  }

  async function generateCode(file) {
    const reader = new FileReader();
    reader.onload = function (e) {
      // 在onload函数中获取最后的内容
      setWpfCode(e.target.result);

    };
    reader.readAsText(file);
  }
  function CSMigrationHandler() {
    const otherFileList = fileList.filter((f) => {
      let t = f.name.split(".");
      return f !== currentFile && t[t.length - 1] === "cs"
    });

    if (otherFileList.length === 0) {
      requestMigration("cs", []);
      return;
    }

    const otherCodeList = [];
    otherFileList.forEach((f) => {
      const reader = new FileReader();
      reader.onload = function (e) {
        otherCodeList.push(e.target.result);
        if (otherCodeList.length === otherFileList.length) {
          requestMigration("cs", otherCodeList);
        }
      };
      reader.readAsText(f)
    });
  }
  function migrate() {
    if (wpfCode === "") {
      alert("请选择文件");
      return;
    }

    if (currentType === "xaml") {
      requestMigration("xaml", []);
    } else if (currentType === "cs") {
      CSMigrationHandler();
    } else {
      console.log("error in migrate()");
    }
  }

  async function requestMigration(typeSuffix, otherFileCodes) {
    let data = {
      TypeSuffix: typeSuffix,
      Code: wpfCode,
      OtherFileCodes: otherFileCodes,
      Rules: rules
    };
    let res = await axios.post(`${server}/Migration/`, data);
    setAvaCode(res.data);
  }

  function copy() {
    copyTextToClipboard(avaCode)
      .catch((err) => {
        alert("复制失败！");
      });
  }

  async function copyTextToClipboard(text) {
    if ("clipboard" in navigator) {
      return await navigator.clipboard.writeText(text);
    } else {
      return document.execCommand('copy', true, text);
    }
  }

  return (
    <Box sx={{ padding: "0 30px 5px 30px", backgroundColor: "#f5f5f5"}}>
      <Grid container sx={{ minHeight: "540px", border: "solid #c2c2c2 1px", backgroundColor: "white"}}>
        <Grid item xs={2} sx={{ borderRight: "solid #c2c2c2 1px"}}>
          <DirectoryViewer
            projectTree={projectTree}
            onFileChange={OnFileChange}
            onFileSelect={selectFile}
          />
        </Grid>
        <Grid item xs={10} >
          <InfoBar canMigrate={currentFile !== null} onMigrate={migrate} onCopy={copy}/>
          <DiffViewer
            leftCode={wpfCode.replaceAll("\r\n", "\n").replaceAll(/(\s)*\n(\n)*/g, "\n")}
            rightCode={avaCode}
            type={currentType} />
        </Grid>
        <Grid item xs={12} sx={{borderTop: "solid #c2c2c2 1px"}}>
          <RuleManager rules={rules} addRules={setRules} />
        </Grid>
      </Grid>
    </Box>

  );
}
