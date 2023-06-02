import * as React from "react";
import {useEffect, useState} from "react";
import {
  Box,
  Button,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  Checkbox,
  ListItemText,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  TextField,
  DialogActions,
  FormControl,
  InputLabel, Select, MenuItem
} from "@mui/material";
import Textarea from '@mui/joy/Textarea';
import PlaylistAddIcon from "@mui/icons-material/PlaylistAdd";
import SaveIcon from "@mui/icons-material/Save";
import DragHandleIcon from "@mui/icons-material/DragHandle";
import AddIcon from "@mui/icons-material/Add";
import {SortableContainer, SortableElement} from "react-sortable-hoc";
import {arrayMoveImmutable} from "array-move";
import PropTypes from "prop-types";



export default function RuleManager(props) {
  const { rules, addRules } = props;
  const [items, setItems] = useState([]);

  const [open, setOpen] = React.useState(false);
  const [mode, setMode] = React.useState("");
  useEffect(() => {
    setItems(rules.map((curr) => ({ ...curr, isChecked: true, key: curr.name })));
  }, [rules]);
  const handleSelectChange = (event) => {
    setMode(event.target.value);
  };
  const handleClickOpen = () => {
    setOpen(true);
    console.log(rules);
    console.log(items);
  };

  const handleClose = () => {
    setOpen(false);
  };

  const onSortEnd = ({oldIndex, newIndex}) => {
      setItems(arrayMoveImmutable(items, oldIndex, newIndex));
  };

  const handleToggle = (itemKey) => () => {
    console.log(itemKey);
    const newItems = items.map((curr) => {
      if (curr.key === itemKey) {
        return {...curr, isChecked: !curr.isChecked };
      } else {
        return curr;
      }
    });
    setItems(newItems);
  };

  const SortableItem = SortableElement(({index, value}) => (
      <ListItem
        key={`item-${value.key}`}
        sx={{borderBottom: "solid #c2c2c2 1px"}}
        disablePadding
        secondaryAction={
          <IconButton edge="end" aria-label="delete">
            <DragHandleIcon />
          </IconButton>
        }
      >
        <ListItemButton role={undefined} onClick={handleToggle(value.key)} dense>
          <ListItemIcon>
            <Checkbox
              edge="start"
              checked={value.isChecked}
              tabIndex={-1}
              disableRipple
            />
          </ListItemIcon>
          <ListItemText primary={value.name} />
        </ListItemButton>
      </ListItem>
  ));

  const SortableList = SortableContainer(({items}) => {
    return (
      <List
        sx={{ width: "100%", bgcolor: 'background.paper' }}
      >
        {items.map((item, index) => (
          <SortableItem
            key={`item-${item.key}`}
            index={index}
            value={item}
          />
        ))}
        <ListItem
          key={"item-add"}
          disablePadding
        >
          <ListItemButton role={undefined} onClick={handleClickOpen}>
            <ListItemIcon>
              <PlaylistAddIcon />
            </ListItemIcon>
            <ListItemText primary="添加自定义规则" />
          </ListItemButton>
        </ListItem>
      </List>
    );
  });

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          alignItems: "end",
          borderBottom: "solid #c2c2c2 1px",
          padding: "0 5px 0 15px"
        }}
      >
        <Typography variant="button" display="block" gutterBottom sx={{flex: "1"}}>
          规则管理区
        </Typography>
        <Button color="primary" component="label" endIcon={<SaveIcon />} onClick={() => addRules(items.map((curr) => ({ ...curr, isChecked: undefined, key: undefined })))}>
          保存
        </Button>
      </Box>
      <Box sx={{padding: "0 20px"}}>
        <SortableList items={items} onSortEnd={onSortEnd}/>
      </Box>
      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>自定义规则</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            id="name"
            label="规则名称"
            fullWidth
            variant="standard"
          />
          <TextField
          autoFocus
          margin="dense"
          id="trigger"
          label="触发器"
          fullWidth
          variant="standard"
          />
          <Box sx={{padding: "10px 0"}}>
            <FormControl fullWidth variant="standard">
              <InputLabel id="select-label">应用模式</InputLabel>
              <Select
                labelId="select-label"
                id="mode"
                value={mode}
                label="应用模式"
                onChange={handleSelectChange}
              >
                <MenuItem value={""}><br/></MenuItem>
                <MenuItem value={"onLoop"}>单次模式</MenuItem>
                <MenuItem value={"loop"}>穷尽模式</MenuItem>
              </Select>
            </FormControl>
          </Box>
          <Box sx={{padding: "10px 0"}}>
            <Button variant="outlined" startIcon={<AddIcon />}>
              添加改动
            </Button>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>取消</Button>
          <Button onClick={handleClose}>添加</Button>
        </DialogActions>
      </Dialog>
      {/*<Box sx={{padding: "5px"}}>
        <Textarea
          minRows={5}
          placeholder="在此输入json格式的规则数组"
          onChange={(e) => setRuleList((e.target.value))}
        />}
      </Box>*/}
    </Box>
  );
}

RuleManager.prototype = {
  rules: PropTypes.array.isRequired,
  addRules: PropTypes.func.isRequired
}

