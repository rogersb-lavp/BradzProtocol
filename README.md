# Bradz Protocol
### Parallel Port Communication Specification
**PC to Test Fixture Interface Protocol**

---

## Overview

The **Bradz Protocol** is a low-level communication specification for transferring data between a host PC and an attached test fixture via the standard PC parallel port (IEEE 1284).

Developed in 2007 by Bradley Rogers, the protocol defines two independent encoding schemes — a compact binary frame format for production use, and a fully ASCII-compliant format for development and debugging. The test fixture target implements a 16-bit register architecture with address auto-increment behavior.

This protocol was designed for use in hardware test environments where a PC controller needs to read and write registers on a custom test fixture with minimal hardware overhead.

---

## Two Protocol Schemes

### Scheme A — Binary Frame Protocol
- Compact binary-formatted frames
- High efficiency, minimal overhead
- Address auto-increment after each register write
- Maximum 16 bytes per transaction
- 8-bit checksum + EOF frame termination
- Suitable for production and high-speed transfer

### Scheme B — ASCII Character Protocol
- Fully compliant with the ASCII character set
- Human-readable — suitable for terminal monitoring and logging
- 16-bit registers encoded as 4-character hex ASCII strings
- Hex-encoded checksum
- Suitable for development, debugging, and field diagnostics

---

## Frame Structure (Binary Protocol)

```
  WORD 0 (First Word)                     WORD 1 (Second Word)
  +---------+---------+--------+------+   +---------+---------+----------+
  | SOF[7]  | ADR[6:2]| WD[1] | WR[0]|   | WDF[7]  | CMD[6:5]| BC[4:2]  |
  +---------+---------+--------+------+   +---------+---------+----------+
  Bit 7      Bits 6:2  Bit 1   Bit 0       Bit 7     Bits 6:5  Bits 4:2
```

| Field | Bits | Description |
|-------|------|-------------|
| SOF | [7] | Start of Frame marker |
| ADR | [6:2] | Target register address — auto-increments after each write |
| WD | [1] | Word/Data flag — always 0 in first word |
| WR | [0] | Direction: 1 = Write to slave, 0 = Read request |
| WDFlag | [7] | Word/Data flag — always 1 in second word |
| CMD | [6:5] | Command field — reserved for future use |
| BC | [4:2] | Byte Count — bytes remaining (not counting current byte) |

> **Auto-Increment Note:** If five registers are written starting at R4, the hardware address pointer advances to R9 upon completion.

---

## ASCII Protocol Example

```
  C  W              Command = Write
  A  0004           Address = Register 4
  L  0001           Length  = 1 register
     1A2B           Data    = 0x1A2B
  E  <checksum>     End of File + checksum
```

---

## Lab Bench Setup

The protocol operates over a standard DB-25 parallel port cable connecting the PC to the test fixture. The test fixture contains:

- 16-bit addressable registers
- LED register indicators
- Screw terminal connections for signal access

A digital multimeter is recommended on the bench for voltage verification during development.

```
  [PC]---[DB-25 Ribbon Cable]---[TEST FIXTURE]   [DMM]
   |                                  |
  Parallel Port (LPT)           16-bit Registers
  IEEE 1284                     Auto-Increment Addressing
```

---

## Error Handling

- All transactions include an **8-bit checksum** verified by the receiving device
- All transactions synchronized to the **rising clock edge**
- **Retry cycle** triggered automatically on error detection
- Error recovery for broken READ transactions to be defined in future revision
- Return data follows **IEEE 1284 handshake** — 1 to 16 bytes per READ request

---

## Documents in This Repository

| File | Description |
|------|-------------|
| `BradzProtocol_WithDiagrams.docx` | Full specification with all 5 engineering diagrams embedded |
| `BradzProtocol_Professional.docx` | Clean specification without images — suitable for printing |
| `README.md` | This file |

---

## Figures

| Figure | Description |
|--------|-------------|
| Figure 1 | Lab Bench Setup — PC, DB-25 Ribbon Cable, Test Fixture, Digital Multimeter |
| Figure 2 | Frame Structure Exploded View — WORD 0 and WORD 1 Bit Field Definitions |
| Figure 3 | Combined Reference — Lab Bench Overview, Frame Structure, and Write Transaction Flow |
| Figure 4 | Read Transaction Flow — PC Controller to Test Fixture Sequence Diagram |
| Figure 5 | Timing and Synchronization Chart — Signal Timing, Error Flag, and Retry Cycle |

---

## Specification Summary

| Parameter | Value |
|-----------|-------|
| Interface | PC Parallel Port (IEEE 1284) |
| Target | Test Fixture with 16-bit Registers |
| Max Transfer | 16 bytes per transaction |
| Register Width | 16 bits |
| Address Width | 5 bits (ADR[4:0]) |
| Checksum | 8-bit |
| Clock | Rising edge synchronization |
| Protocol Schemes | Binary Frame, ASCII Character |

---

## Revision History

| Rev | Date | Author | Description |
|-----|------|--------|-------------|
| 1.0 | 2007 | Bradley Rogers | Initial release — Binary Frame and ASCII Character protocols defined |

---

## Future Work

- Define EOM (End of Message) indicator format for complex READ recovery
- Define error recovery procedure for broken READ transactions
- Expand CMD field definitions as command set is implemented
- Define handshake timing requirements for IEEE 1284 compliance
- Add register map for standard test fixture configurations
- Consider CRC-16 upgrade from 8-bit checksum for improved error detection

---

## Author

**Bradley Rogers**  
Hardware and Software Engineer  
GitHub: [rogersb-lavp](https://github.com/rogersb-lavp)

---

*Bradz Protocol Rev 1.0 — Released 2007*
