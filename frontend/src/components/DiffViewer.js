import ReactDiffViewer, {DiffMethod} from "react-diff-viewer";
import {Box} from "@mui/material";
import * as React from "react";
import SyntaxHighlighter, {Prism} from "react-syntax-highlighter";
import {oneLight} from "react-syntax-highlighter/dist/cjs/styles/prism";
import {docco} from "react-syntax-highlighter/dist/cjs/styles/hljs";
import PropTypes from "prop-types";

const lineStyles = {
  backgroundColor: 'transparent',
  padding: '0',
  margin: '0'
};

const diffStyles = {
  variables: {
    dark: {
      highlightBackground: '#fefed5',
      highlightGutterBackground: '#ffcd3c',
    },
  },
  line: {
    padding: '0'
  },
};

export default function DiffViewer(props) {

  const { leftCode, rightCode, type } = props;

  const Highlighter = (str) => (
    <span
      style={{ display: 'inline'}}
    >
      {type === "cs" ?
        <Prism
          language="csharp"
          style={oneLight}
          wrapLines={true}
          wrapLongLines={true}
          customStyle={lineStyles}
          codeTagProps={
            {style: {
                ...oneLight['code[class*="language-"]'],
                ...oneLight[`code[class*="language-csharp"]`],
                backgroundColor: 'transparent'
              }}
          }
        >
          {str}
        </Prism>
        :
        <SyntaxHighlighter
          language="xml"
          style={docco}
          wrapLines={true}
          wrapLongLines={true}
          customStyle={lineStyles}
          codeTagProps={
            {style: {
                ...oneLight['code[class*="language-"]'],
                ...oneLight[`code[class*="language-xml"]`],
                backgroundColor: 'transparent'
              }}
          }>
          {str}
        </SyntaxHighlighter>
      }
    </span>
  );

  return (
    <Box sx={{overflowX: "auto",overflowY: "auto", maxHeight: "500px", fontSize: "13px"}}>
      <ReactDiffViewer
        oldValue={leftCode}
        newValue={rightCode}
        splitView={true}
        compareMethod={DiffMethod.LINES}
        styles={diffStyles}
        renderContent={Highlighter}
      />
    </Box>
  );
}

DiffViewer.propTypes = {
  leftCode: PropTypes.string.isRequired,
  rightCode: PropTypes.string.isRequired,
  type: PropTypes.string.isRequired
};