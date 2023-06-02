import { styled } from '@mui/material/styles';
import {alpha, Collapse} from "@mui/material";
import {TreeItem, treeItemClasses} from "@mui/lab";

import PropTypes from "prop-types";
import { useSpring, animated } from '@react-spring/web';


function TransitionComponent(props) {
  const style = useSpring({
    from: {
      opacity: 0,
      transform: 'translate3d(20px,0,0)',
    },
    to: {
      opacity: props.in ? 1 : 0,
      transform: `translate3d(${props.in ? 0 : 20}px,0,0)`,
    },
  });

  return (
    <animated.div style={style}>
      <Collapse {...props} />
    </animated.div>
  );
}

TransitionComponent.propTypes = {
  in: PropTypes.bool,
};

const StyledTreeItemRoot = styled((props) => (
  <TreeItem {...props} TransitionComponent={TransitionComponent} />
))(({ theme }) => ({
  [`& .${treeItemClasses.iconContainer}`]: {
    '& .close': {
      opacity: 0.3,
    },
  },
  [`& .${treeItemClasses.group}`]: {
    marginLeft: 15,
    paddingLeft: 18,
    borderLeft: `1px dashed ${alpha(theme.palette.text.primary, 0.4)}`,
  },
}));

export default function StyledTreeItem(props) {
  const {
    file,
    nodeId,
    ...other
  } = props;

  const clickHandler = () => {
    if (file.realFile === undefined) {
      return;
    }

    let tmp = file.name.split(".");
    if (tmp.length < 2) {
      return;
    }

    let suffix = tmp[tmp.length - 1];
    if (suffix !== "cs" && suffix !== "xaml") {
      return;
    }

    props.onSelectFile(file.realFile);
  }

  if (file.realFile === undefined) {
    return (
      <StyledTreeItemRoot
        label={file.name}
        nodeId={nodeId}
        {...other}
      >
        {file.childFolders !== undefined
        && file.childFolders.map((folder, i) => <StyledTreeItem nodeId={props.nodeId + "-folder" + i} key={i} file={folder} {...other}/>)}
        {file.childFolders !== undefined
        && file.childFiles.map((file, i) => <StyledTreeItem nodeId={props.nodeId + "-file" + i} key={i} file={file} {...other}/>)}
      </StyledTreeItemRoot>
    );
  } else {
    return (
      <StyledTreeItemRoot
        label={file.name}
        nodeId={nodeId}
        {...other}
        onClick={clickHandler}
      />
    );
  }
}

StyledTreeItem.propTypes = {
  file: PropTypes.object.isRequired,
  nodeId: PropTypes.string.isRequired,
  onSelectFile: PropTypes.func.isRequired
};