from pathlib import Path
p=Path(r"tools\live_state_mem.py")
s=p.read_text(encoding="utf-8")
old='''            gs = reader.game_state(image)
            diag["game_state_instance"] = gs
        except Exception as exc:  # noqa: BLE001 - 诊断不抛出'''
new='''            gs = reader.game_state(image)
            diag["game_state_instance"] = gs
            # 菜单下实例为 0，但 Mono 元数据/类布局仍可读：给出实体库指针链偏移
            cls_pair = getattr(reader, "_game_state_class", None)
            if cls_pair:
                cls = (cls_pair[1] if isinstance(cls_pair, (tuple, list))
                       and len(cls_pair) > 1 else cls_pair)
                diag["game_state_class_ptr"] = (hex(cls) if isinstance(cls, int)
                                                else str(cls))
                try:
                    fields = reader.class_fields(cls)
                    fields = fields[0] if isinstance(fields, tuple) else fields
                    diag["game_state_fields"] = fields
                except Exception as exc2:  # noqa: BLE001
                    diag["fields_error"] = f"{type(exc2).__name__}: {exc2}"
                try:
                    offs = reader.field_offsets(cls, (
                        "m_entityMap", "m_playerMap", "m_choicesMap",
                        "m_gameEntity", "m_options", "m_queuedChoices",
                        "m_availableRacesInBattlegroundsExcludingAmalgam"))
                    diag["entity_db_offsets"] = (offs[1] if isinstance(offs, tuple)
                                                 else offs)
                except Exception as exc3:  # noqa: BLE001
                    diag["offsets_error"] = f"{type(exc3).__name__}: {exc3}"
        except Exception as exc:  # noqa: BLE001 - 诊断不抛出'''
assert old in s; s=s.replace(old,new)
p.write_text(s,encoding="utf-8")
print("patched diagnostics")
